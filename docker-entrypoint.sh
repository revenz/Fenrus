#!/usr/bin/env bash

# set -e makes the script exit when a command fails.
set -e

if [[ -z "${PORT}" ]]; then
    printf "PORT is not set\n"
    exec dotnet Fenrus.dll --urls=http://+:3000 
else
    printf "PORT is set to '${PORT}'\n"
    exec dotnet Fenrus.dll --urls=http://+:${PORT} 
fi