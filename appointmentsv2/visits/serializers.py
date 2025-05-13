from rest_framework import serializers
from .models import DoctorSchedule, AppointmentType, Appointment

class DoctorScheduleCreateSerializer(serializers.ModelSerializer):
    class Meta:
        model = DoctorSchedule
        fields = ['id', 'weekday', 'start_time', 'end_time', 'doctor_id']


class AppointmentTypeCreateSerializer(serializers.ModelSerializer):
    class Meta:
        model = AppointmentType
        fields = ['id', 'type_name', 'duration_minutes', 'price', 'is_online']


class AppointmentCreateSerializer(serializers.ModelSerializer):
    class Meta:
        model = Appointment
        fields = ['id', 'start_time', 'end_time', 'status', 'patient_id', 'doctor_id', 'appointment_type']
