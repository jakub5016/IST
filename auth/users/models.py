import uuid
from django.contrib.auth.models import AbstractBaseUser, BaseUserManager, PermissionsMixin
from django.db import models

ROLE_NAMES = [
    ("patient", "patient"),
    ("doctor", "doctor"),
    ("employee", "employee"),
    ("admin", "admin")
]

ROLE_NAMES_LIST = ['patient', 'doctor', 'employee', 'admin']

class CustomUserManager(BaseUserManager):
    def create_user(self, email, password=None, **extra_fields):
        if not email:
            raise ValueError("Email is required")
        email = self.normalize_email(email)
        user = self.model(email=email, **extra_fields)
        user.set_password(password)
        user.save()
        return user

    def create_superuser(self, email, password=None, **extra_fields):
        extra_fields.setdefault("is_staff", True)
        extra_fields.setdefault("is_superuser", True)
        return self.create_user(email, password, **extra_fields)

class CustomUser(AbstractBaseUser, PermissionsMixin):
    id = models.UUIDField(primary_key=True, default=uuid.uuid4, editable=False)
    email = models.EmailField(unique=True)
    is_active = models.BooleanField(default=True)
    is_staff = models.BooleanField(default=False)
    is_confirmed_email = models.BooleanField(default=False)
    role = models.CharField(max_length=16, choices=ROLE_NAMES, default=ROLE_NAMES_LIST[0])
    related_id = models.UUIDField(null=False, unique=True)

    objects = CustomUserManager()

    USERNAME_FIELD = 'email'

    def __str__(self):
        return self.email


class ChangePasswordCode(models.Model):
    user = models.ForeignKey(CustomUser, on_delete=models.CASCADE)
    value = models.IntegerField()