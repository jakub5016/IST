from sqlalchemy import create_engine, Column, Integer, String, Boolean, ForeignKey, DateTime
from sqlalchemy.orm import sessionmaker, relationship
from sqlalchemy.orm import declarative_base
from datetime import datetime

DATABASE_URL = "postgresql+psycopg2://postgres:ciscoclass@localhost:5432/postgres"

Base = declarative_base()

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

# Tworzymy silnik łączący się z bazą danych
engine = create_engine(DATABASE_URL)

# Tworzymy sesję do bazy danych
SessionLocal = sessionmaker(autocommit=False, autoflush=False, bind=engine)

def create_appointment(db_session):
    # patient = Patient(first_name="Jan", last_name="Kowalski", gender="M", pesel="02281001231", phone="223456789")
    # doctor = Doctor(first_name="Steven", last_name="Strange", specialization="kardiolog")
    # appointment_type = AppointmentType(type_name="Zabieg kardiologiczny", duration_minutes=60, is_online=False)
    # db_session.add(patient)
    # db_session.add(doctor)
    # db_session.add(appointment_type)
    # db_session.commit()
    # db_session.refresh(patient)
    # db_session.refresh(doctor)
    # db_session.refresh(appointment_type)

    appointment = Appointment(patient_id=1,
                              doctor_id=1,
                              appointment_type_id=1,
                              start_time=datetime(2025, 4, 15, 9, 30),
                              end_time=datetime(2025, 4, 15, 10, 30),
                              status=("Zaplanowana"))
    db_session.add(appointment)
    db_session.commit()
    db_session.refresh(appointment)

    return appointment


# Funkcja do pobrania wizyt pacjenta
def get_appointments_for_patient(db_session, patient_id):
    return db_session.query(Appointment).filter(Appointment.patient_id == patient_id).all()


def main():
    db_session = SessionLocal()

    appointment = create_appointment(db_session)

    patient = db_session.get(Patient, appointment.patient_id)
    doctor = db_session.get(Doctor, appointment.doctor_id)
    appointment_type = db_session.get(AppointmentType, appointment.appointment_type_id)

    print(f"Pacjent: {patient.first_name} {patient.last_name}")
    print(f"Doktor: {doctor.first_name} {doctor.last_name}")
    print(f"Typ wizyty: {appointment_type.type_name}")

    patient_appointments = get_appointments_for_patient(db_session, patient.id)
    print("\nWizyty pacjenta:")
    for i in patient_appointments:
        print(i.id)

    db_session.close()


if __name__ == "__main__":
    main()
