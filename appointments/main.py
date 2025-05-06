from fastapi import FastAPI, Depends
from sqlalchemy.orm import Session
import schemas
import crud
from database.database import SessionLocal, engine, Base

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

@app.post("/doctor-schedules/")
def create_doctor_schedule(schedule: schemas.DoctorScheduleCreate, db: Session = Depends(get_db)):
    return crud.create_doctor_schedule(db, schedule)

@app.post("/appointment-types/")
def create_appointment_type(data: schemas.AppointmentTypeCreate, db: Session = Depends(get_db)):
    return crud.create_appointment_type(db, data)

@app.post("/appointment/")
def create_appointment(data: schemas.AppointmentCreate, db: Session = Depends(get_db)):
    return crud.create_appointment(db, data)
