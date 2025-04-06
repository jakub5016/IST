
import json
import os
import threading
import time
import logging

from typing import Dict, Set
from fastapi import HTTPException
from kafka import KafkaProducer, KafkaConsumer, KafkaAdminClient
from kafka.admin import NewTopic
from kafka.errors import KafkaError, TopicAlreadyExistsError, NoBrokersAvailable
from pay import create_order, refund_order
from db import PAYMENTS_URL_COLLECTION

logger = logging.getLogger()

KAFKA_BROKER = os.getenv("KAFKA_BROKER", "kafka:9092")
PAYMENT_TOPIC = "payment_created"
REFUND_TOPIC = "refunded_payment"


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
        *topic,
        bootstrap_servers=KAFKA_BROKER,
        auto_offset_reset="earliest",
        enable_auto_commit=True,
        value_deserializer=lambda x: json.loads(x.decode("utf-8")),
    )

def get_consumer_and_producer():
    while True:
        try:
            producer = create_producer()
            consumer = create_consumer([REFUND_TOPIC, PAYMENT_TOPIC])
            break
        except NoBrokersAvailable:
            logger.warning("Kafka broker not available. Retrying in 5 seconds...")
            time.sleep(5)

        except Exception as e:
            logger.exception(f"Unexpected error: {e}")
            time.sleep(5)

    return consumer, producer


def kafka_consumer_listener(consumer: KafkaConsumer):
    print("Starting Kafka consumer to listen ...")

    for message in consumer:
        uuid = None
        try:
            value = message.value
            topic = message.topic
            value = dict(value)
            uuid = value.get("uuid")

            existing_order = PAYMENTS_URL_COLLECTION.find_one({"uuid": uuid})
            if topic == PAYMENT_TOPIC:
                if existing_order:
                    status = existing_order.get("status")

                    if status == "url_created":
                        print(f"Order with uuid {uuid} already has URL created. Skipping creation.")
                        continue

                    print(f"Order with uuid {uuid} exists but URL is not created. Setting status to in_progress.")
                    PAYMENTS_URL_COLLECTION.update_one(
                        {"uuid": uuid},
                        {"$set": {"status": "in_progress"}}
                    )

                    value_without_uuid = value.copy()
                    value_without_uuid.pop("uuid", None)
                    
                    url = create_order(value_without_uuid)
                    if url:
                        url['uuid'] = uuid
                        url['status'] = "url_created"

                        send_message(url, "payment_url_created")
                        PAYMENTS_URL_COLLECTION.update_one({"uuid": uuid}, {"$set": url})

                        print(f"Payment URL created and updated {value}, {url}")
                    else:
                        print(f"Error creating order for existing record.")
                        send_message({"uuid": uuid}, "payment_url_error")

                    continue  # Move to next message

                value_without_uuid = value.copy()
                value_without_uuid.pop("uuid", None)

                url = create_order(value_without_uuid)
                if url:
                    url['uuid'] = uuid
                    url['status'] = "url_created"

                    send_message(url, "payment_url_created")
                    PAYMENTS_URL_COLLECTION.insert_one(url)

                    print(f"Payment created {value}, {url}")
                else:
                    print(f"Error creating new order, cannot perform PayU operation")
                    PAYMENTS_URL_COLLECTION.delete_one({"uuid": uuid})
                    send_message({"uuid": uuid}, "payment_url_error")
            elif (topic == REFUND_TOPIC) and existing_order:
                status = existing_order.get("status")
                if status == "url_created":
                    orderId = existing_order.get("orderId")
                    refund_order(orderId)
                else:
                    send_message({"uuid": uuid}, "refunded_payment_error")

        except Exception as e:
            print(f"Error processing message {message}: {e}")

            if uuid and topic == PAYMENT_TOPIC:
                PAYMENTS_URL_COLLECTION.delete_one({"uuid": uuid})
                send_message({"uuid": uuid}, "payment_url_error")

            elif topic == REFUND_TOPIC:
                send_message({"uuid": uuid}, "refunded_payment_error")


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