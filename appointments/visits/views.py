import os
import logging
from datetime import timezone, datetime

from rest_framework.decorators import api_view
from rest_framework.response import Response
from rest_framework import status

from visits import serializers
from visits.models import Appointment, DoctorSchedule, UsersMapping
from visits.serializers import DoctorScheduleChangeSerializer

from kafka_handle.kafka_handle import send_message

NEW_APPOINTMENT_TOPIC = os.getenv("NEW_APPOINTMENT_TOPIC", "new_appointment")
APPOINTMENT_CANCELED_TOPIC = os.getenv("APPOINTMENT_CANCELED_TOPIC", "appointment_cancelled")

@api_view(['GET'])
def healthcheck(request):
    return Response({"message": "OK"})

@api_view(['PUT'])
def doctor_schedule(request):
    role = request.headers.get('x-jwt-role')

    if role == 'employee':
        data = request.data.copy()
        serializer = DoctorScheduleChangeSerializer(data)
        if serializer.is_valid():
            try:
                schedule = DoctorSchedule.objects.get(weekday=data.weekday, doctor_id=data.doctor_id)
                schedule.start_time = data.start_time
                schedule.end_time = data.end_time
                schedule.save()
                return Response(
                    {
                        'schedule_id': str(schedule.id), 
                        'start_time': schedule.start_time,
                        'end_time': schedule.end_time
                    }, status=status.HTTP_200_OK
                )
            except DoctorSchedule.DoesNotExist:
                return Response({'detail': 'Schedule not found.'}, status=status.HTTP_404_NOT_FOUND)

@api_view(['GET', 'POST'])
def appointment(request):
    related_id = request.headers.get('x-jwt-related-id')
    role = request.headers.get('x-jwt-role')
    if request.method == 'POST':
        data = request.data.copy()
        if role == 'patient':
            doctor_id = data.get('doctor_id')
            if not doctor_id:
                return Response({'detail': 'Field doctor_id is required.', "data": data}, status=status.HTTP_400_BAD_REQUEST)
            data['patient_id'] = related_id
        else:
            patient_id = data.get('patient_id')
            if not patient_id:
                return Response({'detail': 'Field patient_id is required.', "data": data}, status=status.HTTP_400_BAD_REQUEST)
            data['doctor_id'] = related_id

        serializer = serializers.AppointmentCreateSerializer(data=data)
        if serializer.is_valid():
            try:
                patient = UsersMapping.objects.get(id=data['patient_id'])
            except UsersMapping.DoesNotExist:
                return Response(
                    {
                        "detail":"Patient with this ID not found", 
                        "data":
                            {
                                "patient_id": 
                                data['patient_id']
                            }
                    }, status=status.HTTP_404_NOT_FOUND) 
                # Add handling when patient does not exist
            
            try:
                doctor = UsersMapping.objects.get(id=data['doctor_id'])
            except UsersMapping.DoesNotExist:
                return Response(
                    {
                        "detail":"Doctor with this ID not found", 
                        "data":
                            {
                                "doctor_id": 
                                data['doctor_id']
                            }
                    }, status=status.HTTP_404_NOT_FOUND) 
                # Add handling when doctor does not exist

            appointment = serializer.save()
            appointment_type = appointment.appointment_type

            start_iso = appointment.start_time.isoformat().replace('+00:00', 'Z') 
            end_iso = appointment.end_time.isoformat().replace('+00:00', 'Z')

            appointment_data = {
                "appointmentId": str(appointment.id),
                "username" :str(patient.email),
                "patientId": str(appointment.patient_id),
                "price": float(appointment_type.price),
                "appointmentType": str(appointment_type.type_name),
                "startTime": str(start_iso),
                "endTime": str(end_iso),
                "doctorEmail": str(doctor.email),
                "patientEmail": str(patient.email)
            }
            send_message(appointment_data, NEW_APPOINTMENT_TOPIC)
            return Response(serializer.data, status=status.HTTP_201_CREATED)
        return Response(serializer.errors, status=status.HTTP_400_BAD_REQUEST)

    if role == 'patient':
        filter_kwargs = {'patient_id': related_id}
    else:
        filter_kwargs = {'doctor_id': related_id}

    appointments = Appointment.objects.filter(**filter_kwargs)
    appointments_list = []


    for appointment in appointments:
        appointments_list.append({
            "id": str(appointment.id),
            "start_time": appointment.start_time,
            "end_time": appointment.end_time,
            "status": appointment.status,
            "patient_id": appointment.patient_id,
            "doctor_id": appointment.doctor_id,
            "appointment_type": appointment.appointment_type.type_name,
        })
    return Response(appointments_list, status=status.HTTP_200_OK)


@api_view(['PUT'])
def appointment_status(request, appointment_id):
    related_id = request.headers.get('x-jwt-related-id')
    role = request.headers.get('x-jwt-role')

    new_status = request.data.get('status')
    if not appointment_id or not new_status:
        return Response({'detail': 'appointment_id and status are required.'}, status=status.HTTP_400_BAD_REQUEST)

    try:
        appointment = Appointment.objects.get(id=appointment_id)
    except Appointment.DoesNotExist:
        return Response({'detail': 'Appointment not found.'}, status=status.HTTP_404_NOT_FOUND)

    if new_status == 'finished':
        if role != 'doctor' or str(related_id) != str(appointment.doctor_id):
            return Response({'detail': 'Unauthorized to finish this appointment.'}, status=status.HTTP_403_FORBIDDEN)
    elif new_status == 'canceled':
        allowed = (
            (role == 'patient' and str(related_id) == str(appointment.patient_id)) or
            (role == 'doctor' and str(related_id) == str(appointment.doctor_id))
        )
        if not allowed:
            return Response({'detail': 'Unauthorized to cancel this appointment.'}, status=status.HTTP_403_FORBIDDEN)
    else:
        return Response({'detail': 'Invalid status.'}, status=status.HTTP_400_BAD_REQUEST)

    appointment.status = new_status
    appointment.save()

    patient = UsersMapping.objects.get(id=appointment.patient_id)
    doctor = UsersMapping.objects.get(id=appointment.doctor_id)

    start_iso = appointment.start_time.isoformat().replace('+00:00', 'Z') 
    end_iso = appointment.end_time.isoformat().replace('+00:00', 'Z')

    if new_status == 'canceled':
        send_message({
            'appointmentId': str(appointment.id),
            'username': str(patient.email),
            'appointmentType': str(appointment.appointment_type.type_name),
            'startTime': str(start_iso),
            'endTime': str(end_iso),
            'patientId': str(appointment.patient_id),
            'patientEmail': str(patient.email),
            'doctorEmail': str(doctor.email),
            "price": int(appointment.appointment_type.price)
        }, APPOINTMENT_CANCELED_TOPIC)

    return Response({'appointmentId': str(appointment.id), 'status': new_status}, status=status.HTTP_200_OK)