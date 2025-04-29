FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS publish

ARG BUILD_CONFIGURATION=Release

WORKDIR /src

COPY PatientService.csproj .

RUN dotnet restore

COPY . .

RUN dotnet publish -c $BUILD_CONFIGURATION -o /app/publish


FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS runtime 

WORKDIR /app

EXPOSE 8080
EXPOSE 8081

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "PatientService.dll"]
