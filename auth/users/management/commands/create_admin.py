from django.core.management.base import BaseCommand, CommandError
from django.contrib.auth import get_user_model
User = get_user_model()
import os
from uuid import uuid4

password = os.getenv("ADMIN_PASS", "admin")

class Command(BaseCommand):
    
    def handle(self, *args, **options):
        if not User.objects.filter(is_staff=True).exists():
            User.objects.create_superuser(
                email="admin@admin.com",
                password=password, 
                related_id=uuid4(), 
                role="admin",
                is_confirmed_email=True
            )
            print("Superuser created.")
        else:
            print("Superuser already exists.")

        try:
            User.objects.create(
                email="pani.kasia@example.com",
                password=password,
                related_id='1d01419d-ac22-446e-b520-f51795a375c0',
                role="employee",
                is_confirmed_email=True
            )
        except Exception as e:
            print(f"Cannot create example employee: {e}")