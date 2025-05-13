import os
import jwt
import datetime
import requests

from rest_framework.response import Response
from django.contrib.auth import authenticate
from django.db import transaction
from users.models import CustomUser
from rest_framework.views import APIView
from rest_framework.response import Response
from rest_framework import status

from kafka_handle.kafka_handle import send_message

from .serializers import RegisterSerializer, LoginSerializer
from .models import ChangePasswordCode

JWT_ALGORITHM = "HS256"
JWT_EXP_DELTA_SECONDS = 36000 # 10h
USER_REGISTER_TOPC = os.getenv("USER_REGISTER_TOPC", "user_registred")

def generate_jwt_token(user):
    resp = requests.request("GET", "http://kong:8001/consumers/loginuser/jwt").json()
    data = resp['data'][0]
    ISS = data['key']
    JWT_SECRET =data['secret']
    JWT_ALGORITHM = data['algorithm']
    payload = {
        "iss": ISS,
        "email": user.email,
        "is_active": user.is_active,
        "is_confirmed_email": user.is_confirmed_email,
        "role": user.role,
        "exp": datetime.datetime.utcnow() + datetime.timedelta(seconds=JWT_EXP_DELTA_SECONDS)
    }
    token = jwt.encode(payload, JWT_SECRET, algorithm=JWT_ALGORITHM)
    return token

class RegistrationView(APIView):
    def post(self, request):
        serializer = RegisterSerializer(data=request.data)
        if serializer.is_valid():
            try:
                with transaction.atomic():
                    user = serializer.save()
                    token = generate_jwt_token(user)
                    kafka_payload = {
                        "username": user.email,
                        "activationLink": f"localhost:8000/auth/confirm_email?uuid={user.id}",
                        "email": user.email
                    }
                    if not send_message(kafka_payload, USER_REGISTER_TOPC):
                        raise Exception("Failed to send Kafka message")
            except Exception as e:
                return Response(
                    {"error": str(e)}, 
                    status=status.HTTP_500_INTERNAL_SERVER_ERROR
                )
            return Response({
                "token": token,
                "email": user.email,
                "is_active": user.is_active,
                "uuid" : user.id
            }, status=status.HTTP_201_CREATED)

        return Response(serializer.errors, status=status.HTTP_400_BAD_REQUEST)

class LoginView(APIView):
    def post(self, request):
        serializer = LoginSerializer(data=request.data)
        if serializer.is_valid():
            email = serializer.validated_data.get("email")
            password = serializer.validated_data.get("password")
            user = authenticate(request, username=email, password=password)
            if user and user.is_confirmed_email:
                token = generate_jwt_token(user)
                return Response({
                    "token": token,
                    "email": user.email,
                    "is_active": user.is_active
                }, status=status.HTTP_200_OK)
            elif not user:
                return Response({"error": "Invalid credentials"}, status=status.HTTP_401_UNAUTHORIZED)
            elif not user.is_confirmed_email:
                return Response({
                    "message": "Please confirm your email address to log in.",
                }, status=403 )
            return Response({"error": "Invalid credentials"}, status=status.HTTP_401_UNAUTHORIZED)
        return Response(serializer.errors, status=status.HTTP_400_BAD_REQUEST)

class ConfirmEmail(APIView):
    def get(self, request):
        uuid = request.query_params.get("uuid")
        if not uuid:
            return Response(
                {"message":"Incorrect data in request"},
                status=400
            )
        try:
            user = CustomUser.objects.get(id=uuid)
        except CustomUser.DoesNotExist:
            return Response({"message":"User not found"}, status=404)

        user.is_confirmed_email = True
        user.save()
        return Response({"message":"Email authenticated, Thank you!"}, status=200)

class ChangePassword(APIView):
    def post(self, request):
        uuid = request.data.get("uuid")
        new_password = request.data.get("newPassword")
        code = request.data.get("code")
        if not any ([uuid, new_password, code]):
            return Response(
                {"message":"Incorrect data in request"},
                status=400
        )
        try:
            user = CustomUser.objects.get(id=uuid)
        except CustomUser.DoesNotExist:
            return Response({"message":"User not found"}, status=404)
        
        is_code_correct = ChangePasswordCode.objects.filter(user=user, value=int(code))
        if is_code_correct:
            user.set_password(new_password)
            user.is_confirmed_email = True
            user.is_active = True
            user.save()
            is_code_correct.delete()
        else:
            return Response({"message": "Code invalid or already used"}, status=401)
        return Response({"message":"Password changed"}, status=200)
       
class HealthCheck(APIView):
    def get(self, request):
        return Response({"message":"healthy"}, status=200)