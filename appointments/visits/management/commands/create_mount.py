from django.core.management.base import BaseCommand, CommandError
from visits.models import AppointmentType

class Command(BaseCommand):
    
    def handle(self, *args, **options):
        try:
            AppointmentType.objects.create(
                type_name="General Checkup",
                duration_minutes=10,
                price=10000,
                is_online=False
            )
        except:
            print("Cannot create 'General Checkup' appointment type")
        try:
            AppointmentType.objects.create(
                type_name="General Checkup Online",
                duration_minutes=10,
                price=10000,
                is_online=True
            )
        except:
            print("Cannot create 'General Checkup Online' appointment type")