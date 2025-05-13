import logging
import os

from fastapi import FastAPI, Request
from models import Order
from fastapi import FastAPI, HTTPException
from contextlib import asynccontextmanager
from db import PAYMENTS_COLLECTION
from kafka_handle import start_kafka_listener, send_message
from consul import register_service_with_consul
from uuid import uuid4

logger = logging.getLogger()


PAYMENT_CREATED_TOPIC = os.getenv("PAYMENT_CREATED_TOPIC", "payment_created")

@asynccontextmanager
async def startup_event(*args, **kwargs):
    """
    FastAPI startup event.
    It triggers the Kafka consumer listener thread.
    """
    register_service_with_consul(service_name="payment_service", service_port=8088)
    start_kafka_listener()
    print("Application startup complete and Kafka listener running.")
    yield

app = FastAPI(lifespan=startup_event)

@app.get("/health")
def health(request: Request):
    incoming = dict(request.headers)
    return {
        "status": "healthy",
        "headers": incoming
    }

@app.post("/add_payment")
async def add_payment(order: Order):
    order_data = order.model_dump()
    order_data['uuid'] = uuid4()
    try:
        result = PAYMENTS_COLLECTION.insert_one(order_data)

        if not result.acknowledged:
            raise HTTPException(status_code=500, detail="Error inserting into database.")

        try:
            order_data_serialized = order_data
            order_data_serialized['uuid'] = str(order_data_serialized['uuid'])
            order_data_serialized.pop("_id")
            method = order_data_serialized.pop("paymentMethod")
            if method == "payu":
                send_message(order_data, PAYMENT_CREATED_TOPIC)
        except Exception as e:
            PAYMENTS_COLLECTION.delete_one({"_id": result.inserted_id})
            raise HTTPException(status_code=500, detail=f"Error sending message: {str(e)}")

        return {"message": "Order created", "order": order_data}

    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Unexpected error: {str(e)}, {order_data}")

@app.get("/payment/{payment_uuid}")
async def get_payment(payment_uuid: str):
    try:
        payment = PAYMENTS_COLLECTION.find_one({"uuid": payment_uuid})
        if payment:
            payment.pop('_id', None)
            return {"message": "Payment found", "payment": payment}
        else:
            raise HTTPException(status_code=404, detail="Payment not found")
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Unexpected error: {str(e)}")
    

@app.get("/refund/{payment_uuid}")
async def refund_payment(payment_uuid: str):
    try:
        payment = PAYMENTS_COLLECTION.find_one({"uuid": payment_uuid})
        if payment:
            payment.pop('_id')
            payment.pop('uuid')
            method = payment.pop("paymentMethod")
            PAYMENTS_COLLECTION.update_one(
                {"uuid": payment_uuid},
                {"$set": {"refunded": True}}
            )
            if method == "payu":
                send_message(payment, "refunded_payment")
            return {"message": "Payment found", "payment": payment}
        else:
            raise HTTPException(status_code=404, detail="Payment not found")
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Unexpected error: {str(e)}")
    