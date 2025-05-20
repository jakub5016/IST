from django.urls import path
from visits import views

urlpatterns = [
    path('doctor-schedules', views.create_doctor_schedule, name='create_doctor_schedule'),
    path('appointment-types', views.create_appointment_type, name='create_appointment_type'),
    path('appointment', views.create_appointment, name='create_appointment'),
    path('health', views.healthcheck, name='healthcheck'),
]
