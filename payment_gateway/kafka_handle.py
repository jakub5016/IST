
import json
import os
import threading
import time
import logging

from typing import Dict, Set
from kafka import KafkaProducer, KafkaConsumer
from kafka.errors import  NoBrokersAvailable
from pay import create_order, refund_order
from db import PAYMENTS_URL_COLLECTION

logger = logging.getLogger()

KAFKA_BROKER = os.getenv("KAFKA_BROKER", "kafka:9092")
PAYMENT_CREATED_TOPIC = os.getenv("PAYMENT_CREATED_TOPIC", "payment_created")
PAYMENT_URL_CREATED_TOPIC = os.getenv("PAYMENT_URL_CREATED_TOPIC", "payment_url_created")
REFUND_REQUESTED_TOPIC = os.getenv("REFUND_REQUESTED_TOPIC", "refunded_payment")
REFUND_ERROR_TOPIC = os.getenv("REFUND_ERROR_TOPIC", "refunded_payment_error")
REFUND_CREATED_TOPIC = os.getenv("REFUND_CREATED_TOPIC", "refund_created")

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
            consumer = create_consumer([REFUND_REQUESTED_TOPIC, PAYMENT_CREATED_TOPIC])
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
            logger.info(f"Got new message: {value}")
            existing_order = PAYMENTS_URL_COLLECTION.find_one({"uuid": uuid})
            if topic == PAYMENT_CREATED_TOPIC:
                if existing_order:
                    status = existing_order.get("status")

                    if status == "url_created":
                        logger.info(f"Order with uuid {uuid} already has URL created. Skipping creation.")
                        continue

                    logger.info(f"Order with uuid {uuid} exists but URL is not created. Setting status to in_progress.")
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

                        send_message(url, PAYMENT_URL_CREATED_TOPIC)
                        PAYMENTS_URL_COLLECTION.update_one({"uuid": uuid}, {"$set": url})

                        logger.info(f"Payment URL created and updated {value}, {url}")
                    else:
                        logger.info(f"Error creating order for existing record.")
                        send_message({"uuid": uuid}, "payment_url_error")

                    continue  # Move to next message

                scrapped_value = value.copy()
                scrapped_value.pop("uuid", None)
                scrapped_value.pop("refunded", None)
                scrapped_value.pop("patientId", None)
                scrapped_value.pop("appointmentId", None)
                url = create_order(scrapped_value)
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
            elif (topic == REFUND_REQUESTED_TOPIC) and existing_order:
                status = existing_order.get("status")
                if status == "url_created":
                    orderId = existing_order.get("orderId")
                    if refund_order(orderId):
                        send_message({"uuid": uuid}, REFUND_CREATED_TOPIC)
                    else:
                        raise Exception
                else:
                    send_message({"uuid": uuid}, REFUND_ERROR_TOPIC)

        except Exception as e:
            print(f"Error processing message {message}: {e}")

            if uuid and topic == PAYMENT_CREATED_TOPIC:
                PAYMENTS_URL_COLLECTION.delete_one({"uuid": uuid})
                send_message({"uuid": uuid}, "payment_url_error")

            elif topic == REFUND_REQUESTED_TOPIC:
                send_message({"uuid": uuid}, REFUND_ERROR_TOPIC)


def start_kafka_listener():
    listener_thread = threading.Thread(target=kafka_consumer_listener, daemon=True, args=(CONSUMER,))
    listener_thread.start()

def send_message(message:Dict[str, any], topic:str):
    future = PRODUCER.send(topic, message)
    try:
        record_metadata = future.get(timeout=10) 
        logger.info(f"Produced message: {message} to topic {record_metadata.topic}")
        return record_metadata
    except Exception as e:
        return None


CONSUMER, PRODUCER = get_consumer_and_producer()