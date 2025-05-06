from sqlalchemy.orm import Session
import models
import schemas

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
