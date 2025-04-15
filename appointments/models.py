from sqlalchemy import create_engine, Column, Integer, String, Boolean, ForeignKey, DateTime, Time
from sqlalchemy.dialects.postgresql import UUID
import uuid
from database import Base

class Patient(Base):
    __tablename__ = 'patients'

    id = Column(Integer, primary_key=True, index=True)
    first_name = Column(String, index=True)
    last_name = Column(String, index=True)
    gender = Column(String, index=True)
    pesel = Column(String, unique=True, index=True)
    phone = Column(String, unique=True, index=True)

class Doctor(Base):
    __tablename__ = 'doctors'

    id = Column(Integer, primary_key=True, index=True)
    first_name = Column(String, index=True)
    last_name = Column(String, index=True)
    specialization = Column(String, index=True)

class DoctorSchedules(Base):
    __tablename__ = 'doctor_schedules'

    id = Column(Integer, primary_key=True, index=True)
    weekday = Column(Integer, index=True)
    start_time = Column(Time, index=True)
    end_time = Column(Time, index=True)
    doctor_id = Column(Integer, ForeignKey('doctors.id'))

class AppointmentType(Base):
    __tablename__ = 'appointment_types'

    id = Column(Integer, primary_key=True, index=True)
    type_name = Column(String, index=True)
    duration_minutes = Column(Integer, index=True)
    is_online = Column(Boolean, index=True)

class Appointment(Base):
    __tablename__ = 'appointments'

    id = Column(Integer, primary_key=True, index=True)
    start_time = Column(DateTime, index=True)
    end_time = Column(DateTime, index=True)
    status = Column(String)
    patient_id = Column(Integer, ForeignKey('patients.id'))
    doctor_id = Column(Integer, ForeignKey('doctors.id'))
    appointment_type_id = Column(Integer, ForeignKey('appointment_types.id'))

