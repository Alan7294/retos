# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["pw2-clase5.csproj", "./"]
RUN dotnet restore "pw2-clase5.csproj"

COPY . .
RUN dotnet publish "pw2-clase5.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS runtime
WORKDIR /app

RUN apk add --no-cache ca-certificates tzdata postgresql-client

COPY --from=build /app/publish .

RUN addgroup -g 1001 -S dotnetuser && \
    adduser -S dotnetuser -u 1001 && \
    chown -R dotnetuser:dotnetuser /app
USER dotnetuser

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Development

EXPOSE 8080

CMD sh -c "echo 'Esperando DB...' && \
  until PGPASSWORD=7294 psql -h db -U postgres -d retos_db -c 'SELECT 1' > /dev/null 2>&1; do sleep 2; done; \
  echo 'DB lista, iniciando app...' && \
  dotnet pw2-clase5.dll"