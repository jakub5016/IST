from django.core.management.base import BaseCommand, CommandError
from visits.models import AppointmentType

class Command(BaseCommand):
    
    def handle(self, *args, **options):
        AppointmentType.objects.create(
            type_name="General Checkup",
            duration_minutes=10,
            price=10000,
            is_online=False
        )