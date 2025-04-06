from kafka import KafkaConsumer, KafkaProducer
from kafka.errors import NoBrokersAvailable
import json
import os
import logging
import time
from pay import create_order

logger = logging.getLogger()
logging.basicConfig(filename='myapp.log', level=logging.INFO)

KAFKA_BROKER = os.getenv("KAFKA_BROKER", "kafka:29092")
REQUEST_TOPIC_PAYMENT = os.getenv("REQUEST_TOPIC_PAYMENT", "payment_requests")
RESPONSE_TOPIC_ORDERS =  os.getenv("REQUEST_TOPIC_PAYMENT", "orders")

def create_producer():
    return KafkaProducer(
        bootstrap_servers=KAFKA_BROKER,
        value_serializer=lambda v: json.dumps(v).encode("utf-8"),
    )


def main():
    while True:
        try:
            consumer = KafkaConsumer(
                REQUEST_TOPIC_PAYMENT,
                bootstrap_servers=KAFKA_BROKER,
                auto_offset_reset="earliest",
                enable_auto_commit=True,
                value_deserializer=lambda x: json.loads(x.decode("utf-8")),
            )

            producer = create_producer()

            logger.info(f"Listening for messages on topic '{REQUEST_TOPIC_PAYMENT}'...")

            for message in consumer:
                logger.info(f"Received message: {message.value}")
                correlation_id = message.value['correlation_id']
                new_order = create_order()
                mess = {"message": new_order, 'correlation_id': correlation_id}
                print(mess)
                producer.send(RESPONSE_TOPIC_ORDERS, mess)
                producer.flush()

        except NoBrokersAvailable:
            logger.warning("Kafka broker not available. Retrying in 5 seconds...")
            time.sleep(5)

        except Exception as e:
            logger.exception(f"Unexpected error: {e}")
            time.sleep(5)


if __name__ == "__main__":
    main()
