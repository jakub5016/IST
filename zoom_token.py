import os
import requests
from dotenv import load_dotenv
from requests.auth import HTTPBasicAuth

load_dotenv()

account_id = os.getenv("ACCOUNT_ID")
client_id = os.getenv("CLIENT_ID")
client_secret = os.getenv("CLIENT_SECRET")

auth_url = 'https://zoom.us/oauth/token'
headers = {'Content-Type': 'application/x-www-form-urlencoded'}
data = {'grant_type': 'account_credentials', 'account_id': account_id}

response = requests.post(auth_url, headers=headers, data=data, auth=HTTPBasicAuth(client_id, client_secret))
access_token = response.json().get('access_token')

