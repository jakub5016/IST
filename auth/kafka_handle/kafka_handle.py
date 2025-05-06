from typing import Dict, Set
from kafka import KafkaProducer, KafkaConsumer
from kafka.errors import NoBrokersAvailable
from django.contrib.auth import get_user_model
from users.models import ChangePasswordCode

import json
import os
import threading
import time
import logging
import string
import random

logging.basicConfig(filemode='a', filename='kafka_logs.log', level=logging.INFO)
logger = logging.getLogger()

KAFKA_BROKER = os.getenv("KAFKA_BROKER", "kafka:9092")
PATIENT_REGISTERED_TOPIC = os.getenv("PATIENT_REGISTERED_TOPIC", "patient_registred") + "_topic"

User = get_user_model()

def generate_random_password(length=12):
    chars = string.ascii_letters + string.digits + string.punctuation
    return ''.join(random.choice(chars) for _ in range(length))

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
            consumer = create_consumer(PATIENT_REGISTERED_TOPIC)
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

            if not value.get('isAccountRegistred'):
                email = value.get("email")
                if email:
                    if not User.objects.filter(email=email).exists():
                        random_password = generate_random_password()
                        user = User.objects.create_user(email=email, password=random_password)
                        code = ChangePasswordCode.objects.create(value=random.randint(0, 1000), user=user)
                        logger.info(f"Code {code.value}")
                    else:
                        logger.info(f"User with email {email} already exists.")
                else:
                    logger.info("Email is missing in the message.")

        except Exception as e:
            logger.info(f"Error processing message {message}: {e}")


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