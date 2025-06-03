from pydantic import BaseModel
from typing import List


class OrderRecord(BaseModel):
    name: str
    quantity: int
    unitPrice: int


class Order(BaseModel):
    description: str
    paymentMethod: str
    totalAmount: int
    products: List[OrderRecord]
    refunded: bool = False
    patientId: str
    appointmentId: str
