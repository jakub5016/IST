from create_meeting import create_zoom_meeting

if __name__ == "__main__":
    meeting = create_zoom_meeting('123456', '2025-06-01T20:00:00Z', '2025-06-01T21:00:00Z')
    if meeting['status'] == "success":
        print(f"Meeting created! ID: {meeting['meeting_id']}")
        print(f"Zoom link: {meeting['join_url']}")
