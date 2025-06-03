from pymongo import MongoClient

MONGO_URI = "mongodb://mongodb:27017"
CLIENT = MongoClient(MONGO_URI, uuidRepresentation="standard")
DB = CLIENT["payment_db"]
PAYMENTS_COLLECTION = DB["payments"]
REFUND_COLLECTION = DB["refund"]
REFUND_ERROR_COLLECTION = DB["refund"]
