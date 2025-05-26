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
    is_online = serializers.BooleanField(write_only=True)

    class Meta:
        model = Appointment
        fields = ['id', 'start_time', 'end_time', 'status', 'patient_id', 'doctor_id', 'appointment_type', 'is_online']

    def validate(self, attrs):
        type_name = attrs.get('appointment_type')
        is_online = attrs.get('is_online')

        try:
            appointment_type = AppointmentType.objects.get(type_name=type_name, is_online=is_online)
        except AppointmentType.DoesNotExist:
            raise serializers.ValidationError(
                f"Appointment type '{type_name}' with is_online={is_online} does not exist."
            )

        attrs['appointment_type'] = appointment_type
        attrs.pop('is_online')
        return attrs

    def create(self, validated_data):
        return super().create(validated_data)

    def to_representation(self, instance):
        rep = super().to_representation(instance)
        rep['appointment_type'] = instance.appointment_type.type_name
        rep['is_online'] = instance.appointment_type.is_online
        return rep
