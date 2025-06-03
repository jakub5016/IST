from typing import Dict
from kafka import KafkaProducer, KafkaConsumer
from kafka.errors import NoBrokersAvailable
from django.contrib.auth import get_user_model
from users.models import ChangePasswordCode, ROLE_NAMES_LIST, IdentityCall

import json
import os
import threading
import time
import logging
import string
import random
from django.db import transaction

logging.basicConfig(filemode="a", filename="kafka_logs.log", level=logging.INFO)
logger = logging.getLogger()

KAFKA_BROKER = os.getenv("KAFKA_BROKER", "kafka:9092")
PATIENT_REGISTERED_TOPIC = os.getenv("PATIENT_REGISTERED_TOPIC", "patient_registred")
EMPLOYEE_HIRED_TOPIC = os.getenv("EMPLOYEE_HIRED_TOPIC", "employee_hired")
USER_REGISTER_TOPC = os.getenv("USER_REGISTER_TOPC", "user_registred")
PASSWORD_CHANGED_TOPIC = os.getenv("PASSWORD_CHANGED_TOPIC", "password_changed")
IDENTITY_CONFIRMED_TOPIC = os.getenv("IDENTITY_CONFIRMED_TOPIC", "identity_confirmed")
USER_CREATION_FAILED_TOPIC = os.getenv(
    "USER_CREATION_FAILED_TOPIC", "user_creation_failed"
)

User = get_user_model()


def generate_random_password(length=12):
    chars = string.ascii_letters + string.digits + string.punctuation
    return "".join(random.choice(chars) for _ in range(length))


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
            consumer = create_consumer(
                [
                    PATIENT_REGISTERED_TOPIC,
                    EMPLOYEE_HIRED_TOPIC,
                    IDENTITY_CONFIRMED_TOPIC,
                ]
            )
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
            logger.info(value)
            if topic == PATIENT_REGISTERED_TOPIC:
                if not value.get("isAccountRegistred"):
                    email = value.get("email")
                    patient_id = value.get("patientId")
                    foregin_id = patient_id
                    if email and patient_id:
                        if not User.objects.filter(email=email).exists():
                            random_password = generate_random_password()
                            call = IdentityCall.objects.filter(
                                patient_id=patient_id
                            ).first()
                            if call:
                                identity_confirmed = True
                            else:
                                identity_confirmed = False
                            try:
                                with transaction.atomic():
                                    user = User.objects.create_user(
                                        email=email,
                                        password=random_password,
                                        related_id=patient_id,
                                        identity_confirmed=identity_confirmed,
                                    )
                                    if call:
                                        call.delete()
                                    code = ChangePasswordCode.objects.create(
                                        value=random.randint(0, 1000), user=user
                                    )
                                    logger.info(f"Code {code.value}")
                                    send_message(
                                        {
                                            "username": email,
                                            "email": email,
                                            "url": "localhost:8000/auth/change_password",
                                            "code": str(code.value),
                                        },
                                        PASSWORD_CHANGED_TOPIC,
                                    )
                            except Exception as e:
                                logger.error(
                                    f"Error during user creation appeared: {e}"
                                )
                                send_message(
                                    {"patientId": patient_id},
                                    USER_CREATION_FAILED_TOPIC,
                                )
                        else:
                            logger.info(f"User with email {email} already exists.")
                    else:
                        logger.info("Email is missing in the message.")
            elif topic == IDENTITY_CONFIRMED_TOPIC:
                patient_id = value.get("patientId")
                try:
                    unauthicated_user = User.objects.get(related_id=patient_id)
                    unauthicated_user.identity_confirmed = True
                    unauthicated_user.save()
                    logger.info(
                        f"Status of user {unauthicated_user.email} changed to {unauthicated_user.identity_confirmed}"
                    )
                except User.DoesNotExist:
                    logger.info(
                        "User does not exist in database, adding identity call to db"
                    )
                    IdentityCall.objects.create(patient_id=patient_id)
                except Exception as e:
                    logger.error(e)

            elif topic == EMPLOYEE_HIRED_TOPIC:
                email = value.get("email")
                role = value.get("role")
                employee_id = value.get("employeeId")
                foregin_id = employee_id
                if role not in ROLE_NAMES_LIST:
                    logger.info("This role does not exist")
                    continue
                if email:
                    if not User.objects.filter(email=email).exists():
                        random_password = generate_random_password()
                        user = User.objects.create_user(
                            email=email,
                            password=random_password,
                            role=role,
                            related_id=employee_id,
                        )
                        code = ChangePasswordCode.objects.create(
                            value=random.randint(0, 1000), user=user
                        )
                        logger.info(f"Code {code.value}")
                    else:
                        logger.info(f"User with email {email} already exists.")
                else:
                    logger.info("Email is missing in the message.")

            if user:
                kafka_payload = {
                    "userId": str(user.id),
                    "username": user.email,
                    "activationLink": f"localhost:8000/auth/confirm_email?uuid={user.id}",
                    "email": user.email,
                    "role": user.role,
                    "relatedId": foregin_id,
                }
                if not send_message(kafka_payload, USER_REGISTER_TOPC):
                    raise Exception("Failed to send Kafka message")
        except Exception as e:
            logger.info(f"Error processing message {message}: {e}")


def start_kafka_listener():
    listener_thread = threading.Thread(
        target=kafka_consumer_listener, daemon=True, args=(CONSUMER,)
    )
    listener_thread.start()


def send_message(message: Dict[str, any], topic: str):
    future = PRODUCER.send(topic, message)
    try:
        record_metadata = future.get(timeout=10)
        logger.info(f"Produced message: {message} to topic {record_metadata.topic}")
        return record_metadata
    except Exception:
        return None


CONSUMER, PRODUCER = get_consumer_and_producer()
