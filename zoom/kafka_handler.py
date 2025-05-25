from typing import Dict
from kafka import KafkaProducer, KafkaConsumer
from kafka.errors import NoBrokersAvailable

import json
import os
import threading
import time
import logging
import requests

from zoom_token import access_token

logger = logging.getLogger(__name__)
logging.basicConfig(level=logging.INFO)

# --- Zoom config ---
admin_email = 'lukaszwasilewski22@gmail.com'
meeting_url = f'https://api.zoom.us/v2/users/{admin_email}/meetings'

headers = {
    'Authorization': f'Bearer {access_token}',
    'Content-Type': 'application/json'
}

def create_zoom_meeting(appointment_id: str, start_time: str, duration: int):
    meeting_details = {
        'topic': f'Wizyta online - {appointment_id}',
        'type': 2,
        'start_time': start_time,
        'duration': duration,
        'timezone': 'Europe/Warsaw',
        'agenda': 'Wizyta online z lekarzem',
        'settings': {
            'host_video': True,
            'participant_video': True,
            'waiting_room': False,
            'join_before_host': True,
            'mute_upon_entry': True
        }
    }

    response = requests.post(meeting_url, headers=headers, data=json.dumps(meeting_details))
    if response.status_code == 201:
        meeting_info = response.json()
        return {
            "status": "success",
            "meeting_id": meeting_info.get("id"),
            "join_url": meeting_info.get("join_url"),
            "start_url": meeting_info.get("start_url"),
        }
    else:
        logger.error(f"Zoom API error: {response.status_code} - {response.text}")
        return {
            "status": "error",
            "code": response.status_code,
            "message": response.text
        }

# --- Kafka setup ---
KAFKA_BROKER = os.getenv("KAFKA_BROKER", "localhost:9092")
NEW_APPOINTMENT_TOPIC = os.getenv("NEW_APPOINTMENT_TOPIC", "new_appointment")
ZOOM_CREATED_TOPIC = os.getenv("ZOOM_CREATED_TOPIC", "zoom_created")

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

            if not appointment.get("is_online"):
                logger.info(f"Wizyta {appointment['appointment_id']} nie jest online")
                continue

            appointment_id = appointment["appointment_id"]
            duration = appointment["duration_minutes"]
            start_time = appointment.get("start_time") or "2025-06-01T12:00:00Z"  # placeholder

            result = create_zoom_meeting(appointment_id, start_time, duration)

            response_payload = {
                "appointment_id": appointment_id,
                "zoom_status": result["status"]
            }

            if result["status"] == "success":
                response_payload.update({
                    "meeting_id": result["meeting_id"],
                    "join_url": result["join_url"],
                    "start_url": result["start_url"]
                })
            else:
                response_payload.update({
                    "error_code": result.get("code"),
                    "error_message": result.get("message")
                })

            send_message(producer, response_payload, ZOOM_CREATED_TOPIC)

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
