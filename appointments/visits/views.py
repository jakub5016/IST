import os
from rest_framework.decorators import api_view
from rest_framework.response import Response
from rest_framework import status
from visits import serializers
from visits.models import AppointmentType, Appointment, DoctorSchedule

from kafka_handle.kafka_handle import send_message

NEW_APPOINTMENT_TOPIC = os.getenv("NEW_APPOINTMENT_TOPIC", "new_appointment")

@api_view(['GET'])
def healthcheck(request):
    return Response({"message": "OK"})

@api_view(['POST'])
def create_doctor_schedule(request):
    serializer = serializers.DoctorScheduleCreateSerializer(data=request.data)
    if serializer.is_valid():
        DoctorSchedule.objects.create(**serializer.validated_data)
        return Response(serializer.data, status=status.HTTP_201_CREATED)
    return Response(serializer.errors, status=status.HTTP_400_BAD_REQUEST)

@api_view(['POST'])
def create_appointment_type(request):
    serializer = serializers.AppointmentTypeCreateSerializer(data=request.data)
    if serializer.is_valid():
        AppointmentType.objects.create(**serializer.validated_data)
        return Response(serializer.data, status=status.HTTP_201_CREATED)
    return Response(serializer.errors, status=status.HTTP_400_BAD_REQUEST)

@api_view(['POST'])
def create_appointment(request):
    serializer = serializers.AppointmentCreateSerializer(data=request.data)
    if serializer.is_valid():
        appointment = Appointment.objects.create(**serializer.validated_data)

        appointment_type = AppointmentType.objects.get(id=appointment.appointment_type_id)

        appointment_data = {
            "appointment_id": str(appointment.id),
            "patient_id": str(appointment.patient_id),
            "price": float(appointment_type.price)
        }

        send_message(appointment_data, NEW_APPOINTMENT_TOPIC)

        return Response(serializer.data, status=status.HTTP_201_CREATED)
    return Response(serializer.errors, status=status.HTTP_400_BAD_REQUEST)
