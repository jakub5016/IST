FROM python:3.10-alpine

ENV PYTHONDONTWRITEBYTECODE 1
ENV PYTHONUNBUFFERED 1

WORKDIR /app

# Install dependencies
COPY requirements.txt /app/
RUN pip install --upgrade pip && pip install -r requirements.txt

# Copy project
COPY . /app/
RUN ["chmod", "+x", "./docker-entrypoint.sh"]

ENTRYPOINT ["sh", "docker-entrypoint.sh"]

CMD ["python" "manage.py" "runserver" "0.0.0.0:8000"]
