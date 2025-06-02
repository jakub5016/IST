import logging
import os

from fastapi import FastAPI, Request
from models import Order
from fastapi import FastAPI, HTTPException
from contextlib import asynccontextmanager
from db import PAYMENTS_COLLECTION, REFUND_COLLECTION
from kafka_handle import start_kafka_listener, send_message
from uuid import uuid4

logger = logging.getLogger()


PAYMENT_CREATED_TOPIC = os.getenv("PAYMENT_CREATED_TOPIC", "payment_created")


@asynccontextmanager
async def startup_event(*args, **kwargs):
    """
    FastAPI startup event.
    It triggers the Kafka consumer listener thread.
    """
    start_kafka_listener()
    print("Application startup complete and Kafka listener running.")
    yield


app = FastAPI(lifespan=startup_event)


@app.get("/health")
def health(request: Request):
    incoming = dict(request.headers)
    return {"status": "healthy", "headers": incoming}


@app.get("/payments")
async def get_payment_by_patient(request: Request):
    try:
        related_id = request.headers.get("x-jwt-related-id")
        if not related_id:
            raise HTTPException(
                status_code=400, detail="Missing x-jwt-related-id header"
            )

        payments_cursor = PAYMENTS_COLLECTION.find({"patientId": related_id})
        payments = []
        for payment in payments_cursor:
            payment.pop("_id", None)
            payments.append(payment)

        return {"payments": payments}

    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Unexpected error: {str(e)}")


@app.get("/payment/{payment_uuid}")
async def get_payment(payment_uuid: str):
    try:
        payment = PAYMENTS_COLLECTION.find_one({"uuid": payment_uuid})
        if payment:
            payment.pop("_id", None)
            return {"payment": payment}
        else:
            raise HTTPException(status_code=404, detail="Payment not found")
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Unexpected error: {str(e)}")


@app.get("/refund/{payment_uuid}")
async def refund_payment(payment_uuid: str):
    try:
        payment = PAYMENTS_COLLECTION.find_one({"uuid": payment_uuid})
        if payment:
            payment.pop("_id")
            payment.pop("uuid")
            method = payment.pop("paymentMethod")
            if method == "payu":
                send_message(payment, "refunded_payment")
                REFUND_COLLECTION.insert_one({"uuid": payment_uuid})
            else:
                PAYMENTS_COLLECTION.update_one(
                    {"uuid": payment_uuid}, {"$set": {"refunded": True}}
                )
            return {"payment": payment}
        else:
            raise HTTPException(status_code=404, detail="Payment not found")
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Unexpected error: {str(e)}")
