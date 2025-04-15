from datetime import datetime, time
from pydantic import BaseModel
from uuid import UUID

class PatientCreate(BaseModel):
    first_name: str
    last_name: str
    gender: str
    pesel: str
    phone: str

class DoctorCreate(BaseModel):
    first_name: str
    last_name: str
    specialization: str

class DoctorScheduleCreate(BaseModel):
    weekday: int
    start_time: time
    end_time: time
    doctor_id: int

class AppointmentTypeCreate(BaseModel):
    type_name: str
    duration_minutes: int
    is_online: bool

class AppointmentCreate(BaseModel):
    start_time: datetime
    end_time: datetime
    status: str
    patient_id: int
    doctor_id: int
    appointment_type_id: int
