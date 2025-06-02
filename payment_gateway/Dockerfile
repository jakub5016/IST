FROM python:3.10-alpine

WORKDIR /app

COPY requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt

# Install cronie for cron support
RUN apk add --no-cache cronie

COPY . .

# Copy entrypoint script
COPY start.sh /start.sh
RUN chmod +x /start.sh

# Copy crontab file
COPY crontab /etc/crontabs/root

CMD ["/start.sh"]
