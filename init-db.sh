#!/bin/bash
# Script de inicialización: restaura el backup de la base de datos Sakila
# dentro del contenedor de PostgreSQL en el primer arranque.
set -e

if [ -f "/docker-entrypoint-initdb.d/sakila.backup" ]; then
  echo ">> Restaurando base de datos Sakila desde sakila.backup ..."
  pg_restore -U "$POSTGRES_USER" -d "$POSTGRES_DB" \
    --clean --if-exists --no-owner --no-privileges \
    /docker-entrypoint-initdb.d/sakila.backup
  echo ">> Restauración completada."
else
  echo ">> No se encontró sakila.backup, se omite la restauración."
fi