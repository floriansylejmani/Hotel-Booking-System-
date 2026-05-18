# ── Build stage ──────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files first so Docker can cache the restore layer
COPY HotelBookingSystem.sln .
COPY src/HotelBooking.Domain/HotelBooking.Domain.csproj             src/HotelBooking.Domain/
COPY src/HotelBooking.Application/HotelBooking.Application.csproj   src/HotelBooking.Application/
COPY src/HotelBooking.Infrastructure/HotelBooking.Infrastructure.csproj src/HotelBooking.Infrastructure/
COPY src/HotelBooking.Persistence/HotelBooking.Persistence.csproj   src/HotelBooking.Persistence/
COPY src/HotelBooking.API/HotelBooking.API.csproj                   src/HotelBooking.API/

RUN dotnet restore src/HotelBooking.API/HotelBooking.API.csproj

# Copy remaining source and publish
COPY . .
RUN dotnet publish src/HotelBooking.API/HotelBooking.API.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# ── Runtime stage ─────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# curl is needed for the container healthcheck
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "HotelBooking.API.dll"]
