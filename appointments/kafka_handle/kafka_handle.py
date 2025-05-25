from typing import Dict
from kafka import KafkaProducer, KafkaConsumer
from kafka.errors import NoBrokersAvailable

import json
import os
import threading
import time
import logging

from visits.models import UsersMapping

logging.basicConfig(filemode='a', filename='kafka_logs.log', level=logging.INFO)
logger = logging.getLogger()

KAFKA_BROKER = os.getenv("KAFKA_BROKER", "localhost:9092")
USER_REGISTER_TOPC = os.getenv("USER_REGISTER_TOPC", "user_registred")

def create_producer():
    return KafkaProducer(
        bootstrap_servers=KAFKA_BROKER,
        value_serializer=lambda v: json.dumps(v).encode("utf-8"),
    )

def create_consumer(topics):
    return KafkaConsumer(
        *topics,
        bootstrap_servers=KAFKA_BROKER,
        auto_offset_reset="earliest",
        enable_auto_commit=True,
        value_deserializer=lambda x: json.loads(x.decode("utf-8")),
    )

def get_consumer_and_producer():
    while True:
        try:
            producer = create_producer()
            consumer = create_consumer([USER_REGISTER_TOPC])
            break
        except NoBrokersAvailable:
            logger.warning("Kafka broker not available. Retrying in 5 seconds...")
            time.sleep(5)
        except Exception as e:
            logger.exception(f"Unexpected error: {e}")
            time.sleep(5)
    return consumer, producer

def kafka_consumer_listener(consumer):
    logger.info("Starting Kafka consumer to listen...")
    if consumer:
        for message in consumer:
            try:
                value = message.value
                topic = message.topic
                logger.info(f"Received message from topic {topic}: {value}")
                user_id = value.get("relatedId")
                role = value.get("role")
                email = value.get("email")
                
                UsersMapping.objects.update_or_create(
                    id=user_id,
                    defaults={
                        "role": role,
                        "email": email
                    }
                )
            except Exception as e:
                logger.exception(f"Error processing Kafka message: {e}")


def start_kafka_listener():
    listener_thread = threading.Thread(target=kafka_consumer_listener, daemon=True, args=(CONSUMER,))
    listener_thread.start()

def send_message(message: Dict[str, any], topic: str):
    future = PRODUCER.send(topic, message)
    try:
        record_metadata = future.get(timeout=10)
        logger.info(f"Sent Kafka message: {message} to topic {record_metadata.topic}")
        return record_metadata
    except Exception as e:
        logger.exception(f"Error sending Kafka message: {e}")
        return None

CONSUMER, PRODUCER = get_consumer_and_producer()
