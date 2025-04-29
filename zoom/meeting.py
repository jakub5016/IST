import json
from zoom_token import *

admin_email = 'lukaszwasilewski22@gmail.com'
meeting_url = f'https://api.zoom.us/v2/users/{admin_email}/meetings'

headers = {
    'Authorization': f'Bearer {access_token}',
    'Content-Type': 'application/json'
}

def create_meeting(topic, start_time, duration):

    meeting_details = {
        'topic': f'{topic}',
        'type': 2,  # Spotkanie zaplanowane
        'start_time': f'{start_time}',
        'duration': duration,
        'timezone': 'Europe/Warsaw',
        'agenda': 'Wizyta online z lekarzem',
        'settings': {
            'host_video': True,
            'participant_video': True,
            'waiting_room': False,
            "join_before_host": True,
            "mute_upon_entry": True
        }
    }

    response = requests.post(meeting_url, headers=headers, data=json.dumps(meeting_details))

    response_code = response.status_code

    if response_code == 201:
        meeting_info = response.json()
        join_url = meeting_info.get("join_url")  # Link dla lekarza i pacjenta
        start_url = meeting_info.get("start_url")  # Link dla admina (opcjonalnie)
        meeting_id = meeting_info.get("id")
        return [response_code, meeting_id, join_url, start_url]
    else:
        return [response_code]

if __name__ == "__main__":
    meeting = create_meeting('Konsultacja lekarska', '2025-04-01T20:00:00Z', 30)
    if meeting[0] == 201:
        print(f"Spotkanie utworzone! ID: {meeting[1]}")
        print(f"Link do dołączenia (dla lekarza i pacjenta): {meeting[2]}")
        print(f"Link dla admina (opcjonalnie): {meeting[3]}")
