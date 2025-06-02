from typing import Dict
from kafka import KafkaProducer, KafkaConsumer
from kafka.errors import NoBrokersAvailable

import json
import os
import threading
import time
import logging

from create_meeting import create_zoom_meeting

logger = logging.getLogger(__name__)
logging.basicConfig(level=logging.INFO)

KAFKA_BROKER = os.getenv("KAFKA_BROKER", "localhost:9092")
NEW_APPOINTMENT_TOPIC = os.getenv("NEW_APPOINTMENT_TOPIC", "new_appointment")
ZOOM_CREATED_TOPIC = os.getenv("ZOOM_CREATED_TOPIC", "zoom_created")
ZOOM_ERROR_TOPIC = os.getenv("ZOOM_ERROR_TOPIC", "zoom_error")

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
            consumer = create_consumer(NEW_APPOINTMENT_TOPIC)
            break
        except NoBrokersAvailable:
            logger.warning("Kafka broker not available. Retrying in 5 seconds...")
            time.sleep(5)
        except Exception as e:
            logger.exception(f"Unexpected error: {e}")
            time.sleep(5)
    return consumer, producer

def kafka_consumer_listener(consumer, producer):
    logger.info("Kafka consumer started and listening...")
    for message in consumer:
        try:
            appointment = message.value
            logger.info(f"Received appointment: {appointment}")

            if not appointment.get("isOnline"):
                logger.info(f"Visit {appointment['appointment_id']} is not online visit.")
                continue

            appointment_id = appointment["appointmentId"]
            start_time = appointment.get("startTime")
            end_time = appointment.get("endTime")
            
            result = create_zoom_meeting(appointment_id, start_time, end_time)

            response_payload = {
                "appointment_id": appointment_id,
                "zoom_status": result["status"]
            }

            if result["status"] == "success":
                response_payload.update({
                    "meetingId": str(result["meeting_id"]),
                    "joinUrl": result["join_url"],
                    "doctorEmail": appointment["doctorEmail"],
                    "patientEmail": appointment["patientEmail"],
                    "startTime": start_time,
                    "appointmentType": str(appointment['appointmentType']),
                    "appointmentId": str(appointment['appointmentId'])
                })
                send_message(producer, response_payload, ZOOM_CREATED_TOPIC)
            else:
                send_message(producer, response_payload, ZOOM_ERROR_TOPIC)

        except Exception as e:
            logger.exception(f"Error processing appointment message: {e}")

def send_message(producer, message: Dict[str, any], topic: str):
    future = producer.send(topic, message)
    try:
        record_metadata = future.get(timeout=10)
        logger.info(f"Sent message to {record_metadata.topic}")
    except Exception as e:
        logger.exception(f"Failed to send Kafka message: {e}")

def start_kafka_listener():
    consumer, producer = get_consumer_and_producer()
    listener_thread = threading.Thread(target=kafka_consumer_listener, args=(consumer, producer), daemon=True)
    listener_thread.start()

if __name__ == "__main__":
    start_kafka_listener()
    while True:
        time.sleep(60)
