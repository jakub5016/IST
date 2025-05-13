import uuid
from django.db import models

class DoctorSchedule(models.Model):
    id = models.UUIDField(primary_key=True, default=uuid.uuid4, editable=False)
    weekday = models.IntegerField(db_index=True)
    start_time = models.TimeField(db_index=True)
    end_time = models.TimeField(db_index=True)
    doctor_id = models.UUIDField()

    class Meta:
        db_table = 'doctor_schedules'

class AppointmentType(models.Model):
    id = models.UUIDField(primary_key=True, default=uuid.uuid4, editable=False)
    type_name = models.CharField(max_length=255, db_index=True)
    duration_minutes = models.IntegerField(db_index=True)
    price = models.DecimalField(max_digits=10, decimal_places=2, db_index=True)
    is_online = models.BooleanField(db_index=True)

    class Meta:
        db_table = 'appointment_types'

class Appointment(models.Model):
    id = models.UUIDField(primary_key=True, default=uuid.uuid4, editable=False)
    start_time = models.DateTimeField(db_index=True)
    end_time = models.DateTimeField(db_index=True)
    status = models.CharField(max_length=255)
    patient_id = models.UUIDField()
    doctor_id = models.UUIDField()
    appointment_type = models.ForeignKey(AppointmentType, on_delete=models.CASCADE)

    class Meta:
        db_table = 'appointments'
