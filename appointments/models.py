from sqlalchemy import Column, Integer, String, Boolean, ForeignKey, DateTime, Time
from sqlalchemy.dialects.postgresql import UUID
import uuid
from database.database import Base

class DoctorSchedules(Base):
    __tablename__ = 'doctor_schedules'

    id = Column(UUID(as_uuid=True), primary_key=True, default=uuid.uuid4, index=True)
    weekday = Column(Integer, index=True)
    start_time = Column(Time, index=True)
    end_time = Column(Time, index=True)
    doctor_id = Column(UUID(as_uuid=True))

class AppointmentType(Base):
    __tablename__ = 'appointment_types'

    id = Column(UUID(as_uuid=True), primary_key=True, default=uuid.uuid4, index=True)
    type_name = Column(String, index=True)
    duration_minutes = Column(Integer, index=True)
    is_online = Column(Boolean, index=True)

class Appointment(Base):
    __tablename__ = 'database'

    id = Column(UUID(as_uuid=True), primary_key=True, default=uuid.uuid4, index=True)
    start_time = Column(DateTime, index=True)
    end_time = Column(DateTime, index=True)
    status = Column(String)
    patient_id = Column(UUID(as_uuid=True))
    doctor_id = Column(UUID(as_uuid=True))
    appointment_type_id = Column(UUID(as_uuid=True), ForeignKey('appointment_types.id'))

