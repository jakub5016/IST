from rest_framework import serializers
from .models import DoctorSchedule, AppointmentType, Appointment

class AppointmentTypeCreateSerializer(serializers.ModelSerializer):
    class Meta:
        model = AppointmentType
        fields = ['id', 'type_name', 'duration_minutes', 'price', 'is_online']


class DoctorScheduleChangeSerializer(serializers.ModelSerializer):
    class Meta:
        model = DoctorSchedule
        fields = ['weekday', 'start_time', 'end_time', 'doctor_id']

class AppointmentCreateSerializer(serializers.ModelSerializer):
    appointment_type = serializers.CharField(write_only=True)

    class Meta:
        model = Appointment
        fields = ['id', 'start_time', 'end_time', 'status', 'patient_id', 'doctor_id', 'appointment_type']

    def validate_appointment_type(self, value):
        try:
            return AppointmentType.objects.get(type_name=value)
        except AppointmentType.DoesNotExist:
            raise serializers.ValidationError(f"Appointment type '{value}' does not exist.")

    def create(self, validated_data):
        appointment_type = validated_data.pop('appointment_type')
        validated_data['appointment_type'] = appointment_type
        return super().create(validated_data)

    def to_representation(self, instance):
        rep = super().to_representation(instance)
        rep['appointment_type'] = instance.appointment_type.type_name
        return rep