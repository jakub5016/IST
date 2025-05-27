import uuid
from django.db import models

APPOINTMENT_STATUS_CHOICES = [
    ('scheduled', 'Scheduled'),
    ('finished', 'Finished'),
    ('canceled', 'Canceled'),
]

ROLE_NAMES = [
    ("patient", "patient"),
    ("doctor", "doctor"),
    ("employee", "employee"),
    ("admin", "admin")
]

ROLE_NAMES_LIST = ['patient', 'doctor', 'employee', 'admin']

class UsersMapping(models.Model):
    id = models.UUIDField(primary_key=True, default=uuid.uuid4)
    role = models.CharField(max_length=16, choices=ROLE_NAMES, default=ROLE_NAMES_LIST[0])
    email = models.EmailField(unique=True)


class DoctorSchedule(models.Model):
    id = models.UUIDField(primary_key=True, default=uuid.uuid4, editable=False)
    weekday = models.IntegerField(db_index=True, null=False)
    start_time = models.TimeField(db_index=True, null=False)
    end_time = models.TimeField(db_index=True, null=False)
    doctor_id = models.UUIDField(null=False)

class AppointmentType(models.Model):
    id = models.UUIDField(primary_key=True, default=uuid.uuid4, editable=False)
    type_name = models.CharField(max_length=255, db_index=True, unique=True, null=False)
    duration_minutes = models.IntegerField(db_index=True)
    price = models.DecimalField(max_digits=10, decimal_places=2, db_index=True)
    is_online = models.BooleanField(db_index=True)

class Appointment(models.Model):
    id = models.UUIDField(primary_key=True, default=uuid.uuid4, editable=False)
    start_time = models.DateTimeField(db_index=True)
    end_time = models.DateTimeField(db_index=True)
    status = models.CharField(max_length=255, choices=APPOINTMENT_STATUS_CHOICES)
    patient_id = models.UUIDField(null=False)
    doctor_id = models.UUIDField(null=False)
    appointment_type = models.ForeignKey(AppointmentType, on_delete=models.CASCADE)
    zoom_link = models.TextField(null=True, blank=True)