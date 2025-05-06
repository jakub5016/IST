from datetime import datetime, time
from pydantic import BaseModel
from uuid import UUID

class DoctorScheduleCreate(BaseModel):
    weekday: int
    start_time: time
    end_time: time
    doctor_id: UUID

class AppointmentTypeCreate(BaseModel):
    type_name: str
    duration_minutes: int
    is_online: bool

class AppointmentCreate(BaseModel):
    start_time: datetime
    end_time: datetime
    status: str
    patient_id: UUID
    doctor_id: UUID
    appointment_type_id: UUID
