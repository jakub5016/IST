from typing import Dict, Set
from fastapi import HTTPException
from kafka import KafkaProducer, KafkaConsumer, KafkaAdminClient
from kafka.admin import NewTopic
from kafka.errors import KafkaError, TopicAlreadyExistsError
from kafka.errors import NoBrokersAvailable

from db import PAYMENTS_COLLECTION

import json
import os
import threading
import time
import logging

logger = logging.getLogger()

KAFKA_BROKER = os.getenv("KAFKA_BROKER", "kafka:9092")
REFUND_ERROR_TOPIC = "refunded_payment_error"


def create_topic(topic):
    try:
        admin_client = KafkaAdminClient(bootstrap_servers=KAFKA_BROKER)
        topic_list = [NewTopic(name=topic, num_partitions=1, replication_factor=1)]
        admin_client.create_topics(new_topics=topic_list, validate_only=False)
        admin_client.close()
    except TopicAlreadyExistsError:
        pass  
    except KafkaError as e:
        raise HTTPException(status_code=500, detail=f"Kafka error: {str(e)}")

def create_producer():
    return KafkaProducer(
        bootstrap_servers=KAFKA_BROKER,
        value_serializer=lambda v: json.dumps(v).encode("utf-8"),
    )

def create_consumer(topic):
    return KafkaConsumer(
        topic,
        bootstrap_servers=KAFKA_BROKER,
        auto_offset_reset="earliest",
        enable_auto_commit=True,
        value_deserializer=lambda x: json.loads(x.decode("utf-8")),
    )

def get_consumer_and_producer():
    while True:
        try:
            producer = create_producer()
            consumer = create_consumer(REFUND_ERROR_TOPIC)
            break
        except NoBrokersAvailable:
            logger.warning("Kafka broker not available. Retrying in 5 seconds...")
            time.sleep(5)

        except Exception as e:
            logger.exception(f"Unexpected error: {e}")
            time.sleep(5)

    return consumer, producer

def kafka_consumer_listener(consumer):
    print("Starting Kafka consumer to listen...")
    for message in consumer:
        try:
            value = message.value
            topic = message.topic

            if topic == REFUND_ERROR_TOPIC:
                uuid = value.get('uuid')
                PAYMENTS_COLLECTION.update_one(
                {"uuid": uuid},
                {"$set": {"refunded": False}}
            )
        except Exception as e:
            print(f"Error processing message {message}: {e}")


def start_kafka_listener():
    listener_thread = threading.Thread(target=kafka_consumer_listener, daemon=True, args=(CONSUMER,))
    listener_thread.start()

def send_message(message:Dict[str, any], topic:str):
    future = PRODUCER.send(topic, message)
    try:
        record_metadata = future.get(timeout=10) 
        logger.info(f"Produced refund message: {message} to topic {record_metadata.topic}")
        return record_metadata
    except Exception as e:
        return None

CONSUMER, PRODUCER = get_consumer_and_producer()