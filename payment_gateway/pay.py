import requests
import os  
import logging

# Public POS as default
CLIENT_ID = os.getenv("client_id", "145227")
CLIENT_SECRET = os.getenv("client_secret", "12f071174cb7eb79d4aac5bc2f07563f")
PAYU_AUTH_URL = "https://secure.snd.payu.com/pl/standard/user/oauth/authorize"
PAYU_ORDER_URL = "https://secure.snd.payu.com/api/v2_1/orders"

logger = logging.getLogger()
logging.basicConfig(filename='myapp.log', level=logging.INFO)

def get_access_token():
    headers = {
        "Content-Type": "application/x-www-form-urlencoded"
    }
    data = {
        "grant_type": "client_credentials",
        "client_id": CLIENT_ID,
        "client_secret": CLIENT_SECRET
    }
    response = requests.post(PAYU_AUTH_URL, data=data, headers=headers)
    if response.status_code == 200:
        return response.json().get("access_token")
    else:
        logger.info(response.status_code)
        logger.info(response.text)

        
def create_order():
    access_token = get_access_token()
    headers = {
        "Authorization": f"Bearer {access_token}",
        "Content-Type": "application/json"
        
    }
    order_data = {
        "customerIp": "127.0.0.1",
        "merchantPosId": CLIENT_ID,
        "description": "Przykładowe zamówienie",
        "currencyCode": "PLN",
        "totalAmount": "1000",
        "products": [
            {
                "name": "Produkt 1",
                "unitPrice": "1000",
                "quantity": "1"
            }
        ]
    }
    response = requests.post(PAYU_ORDER_URL, json=order_data, headers=headers, allow_redirects=False)
    if (response.status_code == 201) or (response.status_code == 302):
        logger.info(response.json())
        return response.json()
    else:
        logger.error(response.status_code)
        logger.error(response.text)
        return response.text