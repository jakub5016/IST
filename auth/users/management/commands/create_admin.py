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
                                          role="employee",
                                          is_confirmed_email=True)
            print("Superuser created.")
        else:
            print("Superuser already exists.")