import uuid

from rest_framework.exceptions import AuthenticationFailed, ValidationError, NotFound
from django.utils.dateparse import parse_datetime
from django.utils.timezone import now, is_aware, make_aware
from datetime import timedelta

from visits.models import Appointment, DoctorSchedule, UsersMapping


def validate_appointment_permissions_data(
    data: dict, role: str, related_id: uuid.UUID, identity_confirmed: bool
) -> dict:
    if role == "patient":
        if "patient_id" in data and data["patient_id"] != str(related_id):
            raise AuthenticationFailed(
                "Cannot create a meeting with a different patient."
            )

        if "doctor_id" not in data:
            raise ValidationError("Field 'doctor_id' is required.")

        if not identity_confirmed:
            if len(Appointment.objects.filter(patient_id=related_id)) > 1:
                raise ValidationError(
                    "Cannot create more than one appointment if user identity is not confirmed"
                )

        data["patient_id"] = str(related_id)
    elif role == "doctor":
        if "doctor_id" in data and data["doctor_id"] != str(related_id):
            raise AuthenticationFailed(
                "Cannot create a meeting with a different doctor."
            )
        if "patient_id" not in data:
            raise ValidationError("Field 'patient_id' is required.")
        data["doctor_id"] = str(related_id)
    elif role not in ("employee", "admin"):
        raise AuthenticationFailed("Invalid role provided.")

    if role not in ("employee", "admin"):
        data["status"] = "scheduled"

    if (data["payment_type"] == "cash") and (
        role not in ("employee", "admin")
    ):  # Chagne to .get()
        raise AuthenticationFailed("Cannot create a meeting with payment type cash")

    return data


def validate_appointment_data(data):
    try:
        patient = UsersMapping.objects.get(id=data["patient_id"])
    except UsersMapping.DoesNotExist:
        raise NotFound("Patient with this ID not found")
    try:
        doctor = UsersMapping.objects.get(id=data["doctor_id"])
    except UsersMapping.DoesNotExist:
        raise NotFound("Doctor with this ID not found")

    start_dt = parse_datetime(data["start_time"])
    end_dt = parse_datetime(data["end_time"])

    if start_dt and not is_aware(start_dt):
        start_dt_aware = make_aware(start_dt)
    else:
        start_dt_aware = start_dt

    if start_dt_aware < (now() + timedelta(hours=1)):
        raise ValidationError(
            f"Appointments must be scheduled at least 1 hour in advance. {start_dt_aware} < {now() + timedelta(hours=1)}"
        )

    weekday = start_dt_aware.weekday()
    data["weekday"] = weekday
    doctor_schedules = DoctorSchedule.objects.filter(
        doctor_id=doctor.id, weekday=weekday
    )
    if not doctor_schedules.exists():
        raise ValidationError(f"Doctor is not available on this day - {weekday}.")

    appointment_start = start_dt.time()
    appointment_end = end_dt.time()

    overlapping_appointments = Appointment.objects.filter(
        doctor_id=doctor.id,
        start_time__lt=end_dt,
        end_time__gt=start_dt,
        status="scheduled",
    )

    if overlapping_appointments.exists():
        raise ValidationError("Doctor already has an appointment during this time.")

    for schedule in doctor_schedules:
        if not (
            (schedule.start_time <= appointment_start)
            and (appointment_end <= schedule.end_time)
        ):
            raise ValidationError("Doctor is not available in this hours.")

    return patient, doctor
