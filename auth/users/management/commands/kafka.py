import json
from django.core.management.base import BaseCommand
from django.contrib.auth import authenticate, get_user_model
from django.db import IntegrityError
from kafka import KafkaConsumer, KafkaProducer
import os

User = get_user_model()
KAFKA_BROKER = os.getenv("KAFKA_BROKER", "kafka:9092")

REQUEST_TOPIC_REGISTER = os.getenv("REQUEST_TOPIC_REGISTER", "register_request")
RESPONSE_TOPIC_REGISTER =  os.getenv("RESPONSE_TOPIC_REGISTER", "register_response")

REQUSET_TOPIC_AUTH = os.getenv("REQUSET_TOPIC_AUTH", "auth_request")
RESPONSE_TOPIC_AUTH =  os.getenv("RESPONSE_TOPIC_AUTH", "auth_response")

class Command(BaseCommand):
    help = 'Listens to Kafka topics for authentication and registration events'

    def handle(self, *args, **kwargs):
        consumer = KafkaConsumer(
            'to_auth',
            'to_register',
            bootstrap_servers=KAFKA_BROKER
            auto_offset_reset='earliest',
            enable_auto_commit=True,
            value_deserializer=lambda v: json.loads(v.decode('utf-8'))
        )
        producer = KafkaProducer(
            bootstrap_servers=KAFKA_BROKER
            value_serializer=lambda v: json.dumps(v).encode('utf-8')
        )
        self.stdout.write(f"Kafka listener started. Listening for messages on '{REQUSET_TOPIC_AUTH}' and '${REQUEST_TOPIC_REGISTER}'...")

        for message in consumer:
            topic = message.topic
            data = message.value

            if topic == REQUSET_TOPIC_AUTH:
                self.handle_auth(data, producer)
            elif topic == REQUEST_TOPIC_REGISTER:
                self.handle_register(data, producer)

    def handle_auth(self, data, producer):
        email = data.get('email')
        password = data.get('password')

        user = authenticate(email=email, password=password)
        if user:
            response = {
                'status': 'success',
                'message': 'User authenticated successfully',
                'user_id': user.id,
                'email': user.email
            }
        else:
            response = {
                'status': 'fail',
                'message': 'Invalid credentials'
            }
        producer.send(RESPONSE_TOPIC_AUTH, response)
        producer.flush()

    def handle_register(self, data, producer):
        email = data.get('email')
        password = data.get('password')

        try:
            user = User.objects.create_user(email=email, password=password)
            response = {
                'status': 'success',
                'message': 'User registered successfully',
                'user_id': user.id,
                'email': user.email
            }
        except IntegrityError:
            response = {
                'status': 'fail',
                'message': 'User already exists'
            }
        except Exception as e:
            response = {
                'status': 'fail',
                'message': str(e)
            }
        producer.send(RESPONSE_TOPIC_REGISTER, response)
        producer.flush()
