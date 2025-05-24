import logging
import requests
import jwt

from fastapi import FastAPI, HTTPException
from contextlib import asynccontextmanager
from kafka_handle import start_kafka_listener
from db import PAYMENTS_URL_COLLECTION
from pay import get_order

logger = logging.getLogger()

def get_client_id(jwt_token):
    resp = requests.request("GET", "http://kong:8001/consumers/loginuser/jwt").json()
    data = resp['data'][0]
    JWT_SECRET =data['secret']
    JWT_ALGORITHM = data['algorithm']
    try:
        decoded_data = jwt.decode(jwt_token, JWT_SECRET, algorithms=[JWT_ALGORITHM])
        return decoded_data
    except jwt.ExpiredSignatureError:
        print("Token has expired")
    except jwt.InvalidTokenError:
        print("Invalid token")

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
def health():
    return {"status": "healthy"}


@app.get("/payment_url/{payment_uuid}")
async def get_payment(payment_uuid: str):
    try:
        payment = PAYMENTS_URL_COLLECTION.find_one({"uuid": payment_uuid})
        if payment:
            payment.pop('_id', None)
            status = payment.get("status")
            if status == "url_created":
                order = get_order(payment.get("orderId"))
                if order:
                    payment['orderPayUData'] = order
                else:
                    payment['orderPayUData'] = "Not Found"
            else:
                payment['orderPayUData'] = "Not Found"
            return {"message": "Payment found", "payment": payment}
        else:
            raise HTTPException(status_code=404, detail="Payment not found")
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Unexpected error: {str(e)}")