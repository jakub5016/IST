#!/bin/bash
echo "Checking for unapplied migrations"
if python manage.py showmigrations --plan | grep '\[ \]'; then
    echo "Apply database migrations"
    python manage.py migrate
else
    echo "No new migrations to apply"
fi

python manage.py create_admin
 
echo "Starting server"
python manage.py runserver 