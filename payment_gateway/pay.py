import requests
import os  
import logging

# Public POS as default
CLIENT_ID = os.getenv("client_id", "145227")
CLIENT_SECRET = os.getenv("client_secret", "12f071174cb7eb79d4aac5bc2f07563f")
PAYU_AUTH_URL = "https://secure.snd.payu.com/pl/standard/user/oauth/authorize"
PAYU_ORDER_ADD_URL = "https://secure.snd.payu.com/api/v2_1/orders"
PAYU_ORDER_GET_URL = "https://secure.snd.payu.com/api/v2_1/orders/{orderId}"
PAYU_REFUND_URL = "https://secure.snd.payu.com/api/v2_1/orders/{orderId}/refunds"

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

        
def create_order(data):
    access_token = get_access_token()
    headers = {
        "Authorization": f"Bearer {access_token}",
        "Content-Type": "application/json"
        
    }
    order_data = {
        "customerIp": "127.0.0.1",
        "merchantPosId": CLIENT_ID,
        "currencyCode": "PLN",
        **data
    }
    response = requests.post(PAYU_ORDER_ADD_URL, json=order_data, headers=headers, allow_redirects=False)
    if (response.status_code == 201) or (response.status_code == 302):
        logger.info(response.json())
        return response.json()
    else:
        logger.error(response.status_code)
        logger.error(response.text)
        return None
    
def get_order(orderId):
    access_token = get_access_token()
    headers = {
        "Authorization": f"Bearer {access_token}",
        "Content-Type": "application/json"
        
    }
    response = requests.get(PAYU_ORDER_GET_URL.format(orderId=orderId), headers=headers)
    if response.status_code == 200:
        return dict(response.json()).get('orders')[0]
    else:
        print(response.text)
        return None

def refund_order(orderId):
    access_token = get_access_token()
    headers = {
        "Authorization": f"Bearer {access_token}",
        "Content-Type": "application/json"
        
    }
    order_data = {
        "refund":{
            "description":f"Refund for order {orderId}"
        }
    }
    response = requests.post(PAYU_REFUND_URL.format(orderId=orderId), json=order_data, headers=headers)

    if response.status_code == 200:
        return dict(response.json())
    else:
        print(response.text)
        return None