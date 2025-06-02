from db import PAYMENTS_URL_COLLECTION
from kafka_handle import send_message
from datetime import datetime

def check_payments_status():
    # Query to find payments that are NOT completed
    incomplete_payments = list(PAYMENTS_URL_COLLECTION.find({"status": {"$ne": "COMPLETED"}}))

    if incomplete_payments:
        message = {
            "timestamp": datetime.utcnow().isoformat(),
            "incomplete_count": len(incomplete_payments),
            "ids": [str(payment["_id"]) for payment in incomplete_payments]
        }
        # Send Kafka message
        send_message(message, "payment_status_check")
        print(f"[INFO] Sent message about {len(incomplete_payments)} incomplete payments.")
    else:
        print("[INFO] All payments are completed.")

if __name__ == "__main__":
    check_payments_status()