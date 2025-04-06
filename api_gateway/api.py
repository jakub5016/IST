import uuid
from fastapi import FastAPI, HTTPException
from fastapi.openapi.utils import get_openapi
import time
from kafka_handle import create_topic, create_producer, create_consumer
import logging
import os
logger = logging.getLogger()

app = FastAPI()

REQUEST_TOPIC_PAYMENT = os.getenv("REQUEST_TOPIC_PAYMENT", "payment_requests")
RESPONSE_TOPIC_ORDERS =  os.getenv("REQUEST_TOPIC_PAYMENT", "orders")

@app.post('/add_payment_request')
def payment_request():
    create_topic(REQUEST_TOPIC_PAYMENT)
    create_topic(RESPONSE_TOPIC_ORDERS)

    correlation_id = str(uuid.uuid4())
    test_message = {"message": "Add payment", "correlation_id": correlation_id}

    producer = create_producer()
    producer.send(REQUEST_TOPIC_PAYMENT, test_message)
    producer.flush()
    producer.close()
    
    consumer = create_consumer(RESPONSE_TOPIC_ORDERS)

    start_time = time.time()
    timeout = 3  # seconds
    order_response = None
    for message in consumer:
        msg = message.value
        if msg.get("correlation_id") == correlation_id:
            logger.info(f"Received order: {msg}")
            order_response = msg
            break
        if time.time() - start_time > timeout:
            logger.warning("Timeout waiting for order response.")
            break
    
    if order_response:
        return order_response
    else:
        return {"error": "No order response received in time."}

@app.get("/kafka/healthcheck")
def kafka_healthcheck():
    create_topic('healthcheck')
    producer = create_producer()
    test_message = {"message": "healthcheck"}
    producer.send("healthcheck", test_message)
    producer.flush()
    producer.close()

    consumer = create_consumer('healthcheck')
    start_time = time.time()
    timeout = 0.2
    for message in consumer:
        if message.value == test_message:
            consumer.close()
            return {"status": "Kafka is healthy"}
        if time.time() - start_time > timeout:
            break
    consumer.close()
    
    raise HTTPException(status_code=500, detail="Kafka message consumption failed")

@app.get("/openapi.json", include_in_schema=False)
def get_open_api_endpoint():
    return get_openapi(
        title=app.title,
        version=app.version,
        description=app.description,
        routes=app.routes,
    )
