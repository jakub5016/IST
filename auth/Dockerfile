FROM python:3.10-alpine

WORKDIR /app

# Install dependencies
COPY requirements.txt /app/
RUN pip install --upgrade pip && pip install -r requirements.txt

# Copy project
COPY . /app/
RUN ["chmod", "+x", "./docker-entrypoint.sh"]

ENTRYPOINT ["sh", "docker-entrypoint.sh"]