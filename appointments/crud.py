from sqlalchemy.orm import Session
import models, schemas
import uuid

def create_patient(db: Session, patient: schemas.PatientCreate):
    db_patient = models.Patient(**patient.dict())
    db.add(db_patient)
    db.commit()
    db.refresh(db_patient)
    return db_patient

def create_doctor(db: Session, doctor: schemas.DoctorCreate):
    db_doctor = models.Doctor(**doctor.dict())
    db.add(db_doctor)
    db.commit()
    db.refresh(db_doctor)
    return db_doctor

def create_doctor_schedule(db: Session, schedule: schemas.DoctorScheduleCreate):
    db_schedule = models.DoctorSchedules(**schedule.dict())
    db.add(db_schedule)
    db.commit()
    db.refresh(db_schedule)
    return db_schedule

def create_appointment_type(db: Session, data: schemas.AppointmentTypeCreate):
    db_type = models.AppointmentType(**data.dict())
    db.add(db_type)
    db.commit()
    db.refresh(db_type)
    return db_type

def create_appointment(db: Session, data: schemas.AppointmentCreate):
    db_appointment = models.Appointment(**data.dict())
    db.add(db_appointment)
    db.commit()
    db.refresh(db_appointment)
    return db_appointment
