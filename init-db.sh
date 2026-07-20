#!/bin/bash
# Restores the Sakila database backup on first container startup.
set -e

BACKUP="/docker-entrypoint-initdb.d/sakila.backup"

if [ -f "$BACKUP" ]; then
  echo ">> Restoring Sakila database from $BACKUP ..."
  pg_restore -U "$POSTGRES_USER" -d "$POSTGRES_DB" \
    --clean --if-exists --no-owner --no-privileges \
    "$BACKUP"
  echo ">> Restore complete."
else
  echo ">> $BACKUP not found — skipping restore."
fi
