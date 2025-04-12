import os
import jwt
import datetime
import requests

from django.views import View
from rest_framework.response import Response
from django.contrib.auth import authenticate
from rest_framework.views import APIView
from rest_framework.response import Response
from rest_framework import status

from .serializers import RegisterSerializer, LoginSerializer

JWT_ALGORITHM = "HS256"
JWT_EXP_DELTA_SECONDS = 3600 # 1h

def generate_jwt_token(user):
    resp = requests.request("GET", "http://kong:8001/consumers/loginuser/jwt").json()
    data = resp['data'][0]
    ISS = data['key']
    JWT_SECRET =data['secret']
    JWT_ALGORITHM = data['algorithm']
    payload = {
        "email": user.email,
        "iss": ISS,
        "is_active": user.is_active,
        "exp": datetime.datetime.utcnow() + datetime.timedelta(seconds=JWT_EXP_DELTA_SECONDS)
    }
    token = jwt.encode(payload, JWT_SECRET, algorithm=JWT_ALGORITHM)
    return token

class RegistrationView(APIView):
    def post(self, request):
        serializer = RegisterSerializer(data=request.data)
        if serializer.is_valid():
            user = serializer.save()
            token = generate_jwt_token(user)
            return Response({
                "token": token,
                "email": user.email,
                "is_active": user.is_active
            }, status=status.HTTP_201_CREATED)
        return Response(serializer.errors, status=status.HTTP_400_BAD_REQUEST)

class LoginView(APIView):
    def post(self, request):
        serializer = LoginSerializer(data=request.data)
        if serializer.is_valid():
            email = serializer.validated_data.get("email")
            password = serializer.validated_data.get("password")
            user = authenticate(request, username=email, password=password)
            if user:
                token = generate_jwt_token(user)
                return Response({
                    "token": token,
                    "email": user.email,
                    "is_active": user.is_active
                }, status=status.HTTP_200_OK)
            return Response({"error": "Invalid credentials"}, status=status.HTTP_401_UNAUTHORIZED)
        return Response(serializer.errors, status=status.HTTP_400_BAD_REQUEST)

class HealthCheck(APIView):
    def get(self, request):
        return Response({"message":"healthy"}, status=200)