from django.urls import path
from .views import RegistrationView, LoginView, HealthCheck, ConfirmEmail, ChangePassword

urlpatterns = [
    path('register', RegistrationView.as_view(), name='register'),
    path('login', LoginView.as_view(), name='login'),
    path('healthcheck', HealthCheck.as_view(), name='healthcheck'),
    path('confirm_email', ConfirmEmail.as_view(), name="confirm_email"),
    path('change_password', ChangePassword.as_view(), name="change_password")
]
