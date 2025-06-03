from pymongo import MongoClient

MONGO_URI = "mongodb://mongodb:27017"
CLIENT = MongoClient(MONGO_URI, uuidRepresentation="standard")
DB = CLIENT["payment_db"]
PAYMENTS_URL_COLLECTION = DB["payments_url"]
