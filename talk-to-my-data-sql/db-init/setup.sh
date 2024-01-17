#!/bin/bash

echo Setting up db

createdb chinook -U postgres
psql -U postgres chinook -1 -f /mnt/db/chinook_pg_serial_proper_naming.sql &>errorlog.txt
psql -U postgres chinook -1 -f /mnt/db/chinook_readonly_user.sql &>errorlog2.txt
