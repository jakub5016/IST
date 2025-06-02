import os
from db import PAYMENTS_URL_COLLECTION
from kafka_handle import send_message
from datetime import datetime, timedelta
from dateutil import parser
import pytz

from payment_gateway.pay import cancel_order, get_order

PAYMENT_OBSOLETE_TOPIC = os.getenv("PAYMENT_OBSOLETE_TOPIC", "payment_obsolete")


def check_payments_status():
    payments = list(PAYMENTS_URL_COLLECTION.find({"status": "url_created"}))

    for payment in payments:
        order = get_order(payment.get("orderId"))
        date_str = order["orderCreateDate"]

        dt = parser.isoparse(date_str)

        now = datetime.now(pytz.utc)

        difference = now - dt

        if (difference > timedelta(hours=1)) and order.status == "NEW":
            status = cancel_order(payment.get("uuid"))
            if status:
                PAYMENTS_URL_COLLECTION.update_one(
                    {"_id": payment["_id"]}, {"$set": {"status": "obsolete"}}
                )
                message = {
                    "payment_id": payment.get("uuid"),
                }
                send_message(message, PAYMENT_OBSOLETE_TOPIC)


if __name__ == "__main__":
    check_payments_status()
