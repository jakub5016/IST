from django.urls import path
from .views import RegistrationView, LoginView, HealthCheck

urlpatterns = [
    path('register', RegistrationView.as_view(), name='register'),
    path('login', LoginView.as_view(), name='login'),
    path('healthcheck', HealthCheck.as_view(), name='healthcheck'),
]
