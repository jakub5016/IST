from typing import Dict
from kafka import KafkaProducer, KafkaConsumer
from kafka.errors import NoBrokersAvailable

import json
import os
import threading
import time
import logging

logger = logging.getLogger(__name__)

KAFKA_BROKER = os.getenv("KAFKA_BROKER", "localhost:9092")
NEW_APPOINTMENT_TOPIC = os.getenv("NEW_APPOINTMENT_TOPIC", "new_appointment")
PAYMENT_CREATED_TOPIC = os.getenv("PAYMENT_CREATED_TOPIC", "payment_created")

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
            consumer = create_consumer(PAYMENT_CREATED_TOPIC)
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
    for message in consumer:
        try:
            value = message.value
            topic = message.topic
            logger.info(f"Received message from topic {topic}: {value}")

            if topic == PAYMENT_CREATED_TOPIC:
                handle_payment_created(value)

        except Exception as e:
            logger.exception(f"Error processing Kafka message: {e}")

def handle_payment_created(data: Dict):
    appointment_id = data.get("appointment_id")
    logger.info(f"Handling payment for appointment ID: {appointment_id}")

    if not appointment_id:
        logger.warning("No appointment_id in payment message")
        return

    # db: Session = SessionLocal()
    # try:
    #     appointment = db.query(Appointment).filter(Appointment.id == appointment_id).first()
    #     if appointment:
    #         appointment.status = "oplacona"
    #         db.commit()
    #         logger.info(f"Appointment {appointment_id} marked as paid.")
    #     else:
    #         logger.warning(f"Appointment {appointment_id} not found.")
    # except Exception as e:
    #     logger.exception(f"Failed to update appointment status: {e}")
    # finally:
    #     db.close()


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
