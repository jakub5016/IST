import os
import random
import jwt
import datetime
import requests
import logging

from rest_framework.response import Response
from django.contrib.auth import authenticate
from django.db import transaction
from django.contrib.auth import get_user_model
from users.models import CustomUser
from rest_framework.views import APIView
from rest_framework.response import Response
from rest_framework import status

from kafka_handle.kafka_handle import send_message

from .serializers import LoginSerializer
from .models import ChangePasswordCode

User = get_user_model()
logging.basicConfig(filemode="a", filename="kafka_logs.log", level=logging.INFO)
logger = logging.getLogger()

JWT_ALGORITHM = "HS256"
JWT_EXP_DELTA_SECONDS = 36000  # 10h
USER_REGISTER_TOPC = os.getenv("USER_REGISTER_TOPC", "user_registred")
PATIENT_REGISTERED_TOPIC = os.getenv("PATIENT_REGISTERED_TOPIC", "patient_registred")
EMPLOYEE_HIRED_TOPIC = os.getenv("EMPLOYEE_HIRED_TOPIC", "employee_hired")
PASSWORD_CHANGED_TOPIC = os.getenv("PASSWORD_CHANGED_TOPIC", "password_changed")
IDENTITY_CONFIRMED_TOPIC = os.getenv("IDENTITY_CONFIRMED_TOPIC", "identity_confirmed")
USER_CREATION_FAILED_TOPIC = os.getenv(
    "USER_CREATION_FAILED_TOPIC", "user_creation_failed"
)
EMPLOYEE_DISMISSED_TOPIC = os.getenv("EMPLOYEE_DISMISSED_TOPIC", "employee_fired")


def generate_jwt_token(user):
    resp = requests.request("GET", "http://kong:8001/consumers/loginuser/jwt").json()
    data = resp["data"][0]
    ISS = data["key"]
    JWT_SECRET = data["secret"]
    JWT_ALGORITHM = data["algorithm"]
    payload = {
        "iss": ISS,
        "email": user.email,
        "is_active": user.is_active,
        "is_confirmed_email": user.is_confirmed_email,
        "role": user.role,
        "related_id": str(user.related_id),
        "identity_confirmed": user.identity_confirmed,
        "exp": datetime.datetime.utcnow()
        + datetime.timedelta(seconds=JWT_EXP_DELTA_SECONDS),
    }
    token = jwt.encode(payload, JWT_SECRET, algorithm=JWT_ALGORITHM)
    return token


class LoginView(APIView):
    def post(self, request):
        serializer = LoginSerializer(data=request.data)
        if serializer.is_valid():
            email = serializer.validated_data.get("email")
            password = serializer.validated_data.get("password")
            user = authenticate(request, username=email, password=password)
            if user and user.is_confirmed_email:
                token = generate_jwt_token(user)
                return Response(
                    {"token": token, "email": user.email, "is_active": user.is_active},
                    status=status.HTTP_200_OK,
                )
            elif not user:
                return Response(
                    {"error": "Invalid credentials"},
                    status=status.HTTP_401_UNAUTHORIZED,
                )
            elif not user.is_confirmed_email:
                return Response(
                    {
                        "message": "Please confirm your email address to log in.",
                    },
                    status=403,
                )
            return Response(
                {"error": "Invalid credentials"}, status=status.HTTP_401_UNAUTHORIZED
            )
        return Response(serializer.errors, status=status.HTTP_400_BAD_REQUEST)


# class RegisterExistingPatient(APIView):
#     def post(self, request):
#         data = request.data
#         email = data.get("email")
#         patient_id = data.get("patientId")
#         password = data.get("password")
#         foregin_id = patient_id
#         if email and patient_id:
#             if not User.objects.filter(email=email).exists():
#                 call = IdentityCall.objects.filter(patient_id=patient_id).first()
#                 if call:
#                     identity_confirmed = True
#                 else:
#                     identity_confirmed = False
#                 try:
#                     with transaction.atomic():
#                         patient_id = RelatedID.objects.get(id=patient_id)
#                         user = User.objects.create_user(
#                             email=email,
#                             password=password,
#                             related_id=patient_id,
#                             identity_confirmed=identity_confirmed,
#                         )
#                         if call:
#                             call.delete()
#                         code = ChangePasswordCode.objects.create(
#                             value=random.randint(0, 1000), user=user
#                         )
#                         logger.info(f"Code {code.value}")
#                         send_message(
#                             {
#                                 "username": email,
#                                 "email": email,
#                                 "url": "localhost:8000/auth/change_password",
#                                 "code": str(code.value),
#                             },
#                             PASSWORD_CHANGED_TOPIC,
#                         )
#                 except Exception as e:
#                     logger.error(f"Error during user creation appeared: {e}")
#                     return Response({"error": e}, status=status.HTTP_400_BAD_REQUEST)
#             else:
#                 logger.info(f"User with email {email} already exists.")
#                 return Response(
#                     {"error": f"User with email {email} already exists."},
#                     status=status.HTTP_400_BAD_REQUEST,
#                 )
#         kafka_payload = {
#             "userId": str(user.id),
#             "username": user.email,
#             "activationLink": f"localhost:8000/auth/confirm_email?uuid={user.id}",
#             "email": user.email,
#             "role": user.role,
#             "relatedId": foregin_id,
#         }
#         if not send_message(kafka_payload, USER_REGISTER_TOPC):
#             raise Exception("Failed to send Kafka message")
#         return Response({}, status=status.HTTP_201_CREATED)


class ConfirmEmail(APIView):
    def get(self, request):
        uuid = request.query_params.get("uuid")
        if not uuid:
            return Response({"message": "Incorrect data in request"}, status=400)
        try:
            user = CustomUser.objects.get(id=uuid)
        except CustomUser.DoesNotExist:
            return Response({"message": "User not found"}, status=404)

        user.is_confirmed_email = True
        user.save()
        return Response({"message": "Email authenticated, Thank you!"}, status=200)


class ChangePassword(APIView):
    def post(self, request):
        uuid = request.data.get("uuid")
        new_password = request.data.get("newPassword")
        code = request.data.get("code")
        if not any([uuid, new_password, code]):
            return Response({"message": "Incorrect data in request"}, status=400)
        try:
            user = CustomUser.objects.get(id=uuid)
        except CustomUser.DoesNotExist:
            return Response({"message": "User not found"}, status=404)

        is_code_correct = ChangePasswordCode.objects.filter(user=user, value=int(code))
        if is_code_correct:
            user.set_password(new_password)
            user.is_confirmed_email = True
            user.is_active = True
            user.save()
            is_code_correct.delete()
        else:
            return Response({"message": "Code invalid or already used"}, status=401)
        return Response({"message": "Password changed"}, status=200)


class HealthCheck(APIView):
    def get(self, request):
        return Response({"message": "healthy"}, status=200)
