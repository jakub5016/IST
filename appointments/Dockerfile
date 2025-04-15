# Wybierz bazowy obraz Pythona
FROM python:3.10

# Ustaw katalog roboczy w kontenerze
WORKDIR /app

# Zainstaluj zależności
COPY requirements.txt /app/
RUN pip install -r /app/requirements.txt

# Skopiuj pliki aplikacji
COPY . /app

# Ustaw zmienne środowiskowe
ENV PYTHONPATH=/app

# Domyślny sposób uruchomienia aplikacji
CMD ["sh"]