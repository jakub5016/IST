from fastapi import HTTPException
from kafka import KafkaProducer, KafkaConsumer, KafkaAdminClient
from kafka.admin import NewTopic
from kafka.errors import KafkaError, TopicAlreadyExistsError
import json
import time
import os

KAFKA_BROKER = os.getenv("KAFKA_BROKER", "kafka:9092")

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
        topic,
        bootstrap_servers=KAFKA_BROKER,
        auto_offset_reset="earliest",
        enable_auto_commit=True,
        value_deserializer=lambda x: json.loads(x.decode("utf-8")),
    )