from typing import Dict, Set
from uuid import uuid4
from kafka import KafkaProducer, KafkaConsumer
from kafka.errors import NoBrokersAvailable
from models import Order, OrderRecord

from db import PAYMENTS_COLLECTION, REFUND_ERROR_COLLECTION, REFUND_COLLECTION

import json
import os
import threading
import time
import logging

logging.basicConfig(filemode="a", filename="kafka_logs.log", level=logging.INFO)
logger = logging.getLogger()

KAFKA_BROKER = os.getenv("KAFKA_BROKER", "kafka:9092")
REFUND_ERROR_TOPIC = os.getenv("REFUND_ERROR_TOPIC", "refunded_payment_error")
NEW_APPOINTMENT_TOPIC = os.getenv("NEW_APPOINTMENT_TOPIC", "new_appointment")
APPOINTMENT_CANCELED_TOPIC = os.getenv(
    "APPOINTMENT_CANCELED_TOPIC", "appointment_cancelled"
)
PAYMENT_CREATED_TOPIC = os.getenv("PAYMENT_CREATED_TOPIC", "payment_created")
REFUND_REQUESTED_TOPIC = os.getenv("REFUND_REQUESTED_TOPIC", "refunded_payment")
REFUND_CREATED_TOPIC = os.getenv("REFUND_CREATED_TOPIC", "refund_created")


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
                    REFUND_ERROR_TOPIC,
                    NEW_APPOINTMENT_TOPIC,
                    APPOINTMENT_CANCELED_TOPIC,
                    REFUND_CREATED_TOPIC,
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


def send_payment_created(order):
    order_data_serialized = order
    order_data_serialized["uuid"] = str(order_data_serialized["uuid"])
    order_data_serialized.pop("_id")
    method = order_data_serialized.pop("paymentMethod")
    if method == "payu":
        send_message(order, PAYMENT_CREATED_TOPIC)


def kafka_consumer_listener(consumer):
    print("Starting Kafka consumer to listen...")
    for message in consumer:
        try:
            value = message.value
            topic = message.topic
            logger.info(f"{topic}:{value}")
            if topic == REFUND_ERROR_TOPIC:
                uuid = value.get("uuid")
                PAYMENTS_COLLECTION.update_one(
                    {"uuid": uuid}, {"$set": {"refunded": False}}
                )
                REFUND_ERROR_COLLECTION.insert_one({"uuid": uuid})
            elif topic == REFUND_CREATED_TOPIC:
                uuid = value.get("uuid")
                logger.info(f"Refund created for uuid {uuid}")
                PAYMENTS_COLLECTION.update_one(
                    {"uuid": uuid}, {"$set": {"refunded": True}}
                )
            elif topic == APPOINTMENT_CANCELED_TOPIC:
                appointment_id = value.get("appointmentId")
                if not appointment_id:
                    logger.warning("Missing appointmentId in cancellation message.")
                else:
                    payment = PAYMENTS_COLLECTION.find_one({"uuid": appointment_id})
                    if payment:
                        if (payment.get("refunded") is True) or (
                            REFUND_COLLECTION.find_one({"uuid": appointment_id})
                        ):
                            logger.info(
                                f"Refund already processed (or in progress) for appointment_id: {appointment_id}, skipping refund message."
                            )
                            continue
                        if payment.get("paymentMethod") == "payu":
                            orderData = {"uuid": appointment_id}
                            send_message(orderData, REFUND_REQUESTED_TOPIC)
                            REFUND_COLLECTION.insert_one({"uuid": appointment_id})
                            logger.info(
                                f"Refund requested for payu payment: {orderData}"
                            )
                        else:
                            logger.info(
                                f"No refund required for non-payu payment method: {payment.get('paymentMethod')}"
                            )
                    else:
                        logger.warning(
                            f"No payment found for cancelled appointment_id: {appointment_id}"
                        )

            elif topic == NEW_APPOINTMENT_TOPIC:
                logger.info("New appointment appeared")

                existing = PAYMENTS_COLLECTION.find_one(
                    {"uuid": value["appointmentId"]}
                )
                if existing:
                    logger.warning(
                        f"Payment with uuid {value['appointmentId']} already exists. Skipping insertion."
                    )
                    continue

                record = OrderRecord(
                    name=value["appointmentType"],
                    quantity=1,
                    unitPrice=value["price"],
                )

                new_order = Order(
                    appointmentId=value["appointmentId"],
                    description="",
                    paymentMethod=value["paymentType"],
                    totalAmount=value["price"],
                    products=[record],
                    patientId=value["patientId"],
                ).model_dump()

                new_order["uuid"] = value["appointmentId"]
                logger.info(f"Inserting new order: {new_order}")

                try:
                    result = PAYMENTS_COLLECTION.insert_one(new_order)
                    if result.acknowledged:
                        logger.info("Payment inserted successfully.")
                        if value["paymentType"] == "payu":
                            send_payment_created(new_order)
                    else:
                        logger.error("Insertion failed to acknowledge.")
                except Exception as e:
                    logger.exception(f"Error inserting payment: {str(e)}")

        except Exception as e:
            logger.error(f"Error processing message {message}: {e}")


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
