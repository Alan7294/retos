#!/bin/bash
set -e

echo "Esperando a que PostgreSQL esté disponible..."
until PGPASSWORD=7294 psql -h db -U postgres -d retos_db -c '\q' 2>/dev/null; do
  sleep 1
done

echo "Ejecutando migraciones..."
dotnet ef database update

echo "Aplicación iniciada"