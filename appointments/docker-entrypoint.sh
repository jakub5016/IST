#!/bin/bash
echo "Apply database migrations"
python manage.py migrate

echo "Creating mount config"
python manage.py create_mount

echo "Starting server"
python manage.py runserver