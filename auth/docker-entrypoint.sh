#!/bin/bash
find . -type d -name "migration" | while read dir; do find "$dir" -type f ! -name "__init__.py" -delete; done

echo "Create migrations"
python manage.py makemigrations

echo "Apply database migrations"
python manage.py migrate

echo "Starting server"
python manage.py runserver 