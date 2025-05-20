from django.apps import AppConfig


class KafkaHandleConfig(AppConfig):
    default_auto_field = 'django.db.models.BigAutoField'
    name = 'kafka_handle'

    def ready(self):
        from kafka_handle.kafka_handle import start_kafka_listener
        import os
        if os.environ.get('RUN_MAIN', None) != 'true':
            start_kafka_listener()
