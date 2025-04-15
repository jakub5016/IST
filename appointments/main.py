from fastapi import FastAPI, Depends
from sqlalchemy.orm import Session
import models
import schemas
import crud
from database import SessionLocal, engine, Base

# Tworzenie tabel
Base.metadata.create_all(bind=engine)

app = FastAPI(title="Appointment Service")

# Zależność do sesji bazy
def get_db():
    db = SessionLocal()
    try:
        yield db
    finally:
        db.close()

# DEBUG ONLY
@app.post("/patients/")
def create_patient(patient: schemas.PatientCreate, db: Session = Depends(get_db)):
    return crud.create_patient(db, patient)

# DEBUG ONLY
@app.post("/doctors/")
def create_doctor(doctor: schemas.DoctorCreate, db: Session = Depends(get_db)):
    return crud.create_doctor(db, doctor)

# DEBUG ONLY
@app.post("/doctor-schedules/")
def create_doctor_schedule(schedule: schemas.DoctorScheduleCreate, db: Session = Depends(get_db)):
    return crud.create_doctor_schedule(db, schedule)

@app.post("/appointment-types/")
def create_appointment_type(data: schemas.AppointmentTypeCreate, db: Session = Depends(get_db)):
    return crud.create_appointment_type(db, data)

@app.post("/appointments/")
def create_appointment(data: schemas.AppointmentCreate, db: Session = Depends(get_db)):
    return crud.create_appointment(db, data)
