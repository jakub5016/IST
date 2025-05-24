from django.urls import path
from visits import views

urlpatterns = [
    path('appointment', views.appointment, name='appointment'),
    path('health', views.healthcheck, name='healthcheck'),
    path('appointments/<uuid:appointment_id>/status/', views.appointment_status, name='appointment-status'),
]
