import os
import json
import requests
import logging
from datetime import datetime

from zoom_token import access_token

logger = logging.getLogger(__name__)
logging.basicConfig(level=logging.INFO)

ADMIN_EMAIL = os.getenv("ADMIN_EMAIL", "lukaszwasilewski22@gmail.com")
meeting_url = f'https://api.zoom.us/v2/users/{ADMIN_EMAIL}/meetings'

headers = {
    'Authorization': f'Bearer {access_token}',
    'Content-Type': 'application/json'
}

def create_zoom_meeting(appointment_id: str, start_time: str, end_time: str):
    # Zakładamy, że start_time i end_time są w formacie ISO 8601 np. "2025-06-01T12:00:00Z"
    start_dt = datetime.strptime(start_time, "%Y-%m-%dT%H:%M:%SZ")
    end_dt = datetime.strptime(end_time, "%Y-%m-%dT%H:%M:%SZ")
    duration = int((end_dt - start_dt).total_seconds() / 60)

    meeting_details = {
        'topic': f'Wizyta online - {appointment_id}',
        'type': 2,
        'start_time': start_time,
        'duration': duration,
        'timezone': 'Europe/Warsaw',
        'agenda': 'Wizyta online z lekarzem',
        'settings': {
            'host_video': True,
            'participant_video': True,
            'waiting_room': False,
            'join_before_host': True,
            'mute_upon_entry': True
        }
    }

    response = requests.post(meeting_url, headers=headers, data=json.dumps(meeting_details))
    if response.status_code == 201:
        meeting_info = response.json()
        return {
            "status": "success",
            "meeting_id": meeting_info.get("id"),
            "join_url": meeting_info.get("join_url"),
            "start_url": meeting_info.get("start_url"),
            "start_time": meeting_info.get("start_time"),
        }
    else:
        logger.error(f"Zoom API error: {response.status_code} - {response.text}")
        return {
            "status": "error",
            "code": response.status_code,
            "message": response.text
        }