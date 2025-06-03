from typing import Dict
from kafka import KafkaProducer, KafkaConsumer
from kafka.errors import NoBrokersAvailable

import json
import os
import threading
import time as t
import logging
from datetime import time
from django.db import transaction

from visits.models import Appointment, DoctorSchedule, ForbiddenUUID, UsersMapping


logging.basicConfig(filemode="a", filename="kafka_logs.log", level=logging.INFO)
logger = logging.getLogger()

KAFKA_BROKER = os.getenv("KAFKA_BROKER", "localhost:9092")
PATIENT_REGISTERED_TOPIC = os.getenv("PATIENT_REGISTERED_TOPIC", "patient_registred")
EMPLOYEE_HIRED_TOPIC = os.getenv("EMPLOYEE_HIRED_TOPIC", "employee_hired")
ZOOM_CREATED_TOPIC = os.getenv("ZOOM_CREATED_TOPIC", "zoom_created")
ZOOM_ERROR_TOPIC = os.getenv("ZOOM_ERROR_TOPIC", "zoom_error")
APPOINTMENT_CANCELED_TOPIC = os.getenv(
    "APPOINTMENT_CANCELED_TOPIC", "appointment_cancelled"
)
USER_CREATION_FAILED_TOPIC = os.getenv(
    "USER_CREATION_FAILED_TOPIC", "user_creation_failed"
)
EMPLOYEE_DISMISSED_TOPIC = os.getenv("EMPLOYEE_DISMISSED_TOPIC", "employee_fired")


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
            consumer = create_consumer(
                [
                    PATIENT_REGISTERED_TOPIC,
                    EMPLOYEE_HIRED_TOPIC,
                    ZOOM_CREATED_TOPIC,
                    ZOOM_ERROR_TOPIC,
                    USER_CREATION_FAILED_TOPIC,
                    EMPLOYEE_DISMISSED_TOPIC,
                ]
            )
            break
        except NoBrokersAvailable:
            logger.warning("Kafka broker not available. Retrying in 5 seconds...")
            t.sleep(5)
        except Exception as e:
            logger.exception(f"Unexpected error: {e}")
            t.sleep(5)
    return consumer, producer


def kafka_consumer_listener(consumer):
    logger.info("Starting Kafka consumer to listen...")
    if consumer:
        for message in consumer:
            try:
                value = message.value
                topic = message.topic
                logger.info(f"Received message from topic {topic}: {value}")
                if topic == PATIENT_REGISTERED_TOPIC:
                    user_id = value.get("patientId")
                    if not value.get("isAccountRegistred"):
                        email = value.get("email")
                    else:
                        email = None
                    if not ForbiddenUUID.objects.filter(id=user_id).exists():
                        UsersMapping.objects.update_or_create(
                            id=user_id, defaults={"role": "patient", "email": email}
                        )
                    else:
                        logger.info(
                            f"Patient with this uuid was classified as forbidden, uuid: {user_id}"
                        )
                elif topic == EMPLOYEE_HIRED_TOPIC:
                    email = value.get("email")
                    role = value.get("role")
                    user_id = value.get("employeeId")
                    logger.info(value)
                    if role == "doctor":
                        with transaction.atomic():
                            _, is_created = UsersMapping.objects.update_or_create(
                                id=user_id, defaults={"role": "doctor", "email": email}
                            )
                            if is_created:
                                weekday_schedule = [
                                    DoctorSchedule(
                                        doctor_id=user_id,
                                        weekday=i,
                                        start_time=time(9, 0),
                                        end_time=time(16, 0),
                                    )
                                    for i in range(0, 5)
                                ]
                                DoctorSchedule.objects.bulk_create(weekday_schedule)
                elif topic == ZOOM_CREATED_TOPIC:
                    appointment_id = value.get("appointmentId")
                    try:
                        appointment = Appointment.objects.get(id=appointment_id)
                        appointment.zoom_link = value.get("joinUrl")
                        appointment.save()
                    except Appointment.DoesNotExist:
                        logger.error(
                            f"Appointment with id {appointment_id} does not exist but there is zoom meeting!"
                        )

                elif topic == ZOOM_ERROR_TOPIC:
                    appointment_id = value.get("appointmentId")
                    try:
                        appointment = Appointment.objects.get(id=appointment_id)
                        appointment.status = (
                            "cancelled"  # Online meeting without link has no sense
                        )
                        patient = UsersMapping.objects.get(id=appointment.patient_id)
                        doctor = UsersMapping.objects.get(id=appointment.doctor_id)

                        start_iso = appointment.start_time.isoformat().replace(
                            "+00:00", "Z"
                        )
                        end_iso = appointment.end_time.isoformat().replace(
                            "+00:00", "Z"
                        )

                        send_message(
                            {
                                "appointmentId": str(appointment.id),
                                "username": str(patient.email),
                                "appointmentType": str(
                                    appointment.appointment_type.type_name
                                ),
                                "startTime": str(start_iso),
                                "endTime": str(end_iso),
                                "patientId": str(appointment.patient_id),
                                "patientEmail": str(patient.email),
                                "doctorEmail": str(doctor.email),
                                "price": int(appointment.appointment_type.price),
                            },
                            APPOINTMENT_CANCELED_TOPIC,
                        )
                    except Appointment.DoesNotExist:
                        logger.error(
                            f"Appointment with id {appointment_id} does not exist, but error in zoom appered."
                        )
                elif topic == USER_CREATION_FAILED_TOPIC:
                    patient_id = value.get("patientId")
                    try:
                        user_mapping = UsersMapping.objects.get(id=patient_id)
                        user_mapping.delete()
                        logger.info(
                            f"Succesfully removed patient with {patient_id} from database"
                        )
                    except UsersMapping.DoesNotExist:
                        logger.error(f"Patient with {patient_id} does not exist")
                        ForbiddenUUID.objects.create(id=patient_id)
                elif topic == EMPLOYEE_DISMISSED_TOPIC:
                    user_id = value.get("id")
                    try:
                        mapping = UsersMapping.objects.get(id=user_id)
                    except UsersMapping.DoesNotExist:
                        logger.error(
                            f"User mapping with this id does not exist: {user_id}"
                        )
                    mapping.is_active = False
                    mapping.save()

            except Exception as e:
                logger.exception(f"Error processing Kafka message: {e}")


def start_kafka_listener():
    listener_thread = threading.Thread(
        target=kafka_consumer_listener, daemon=True, args=(CONSUMER,)
    )
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
