#!/usr/bin/env bash

# set -e makes the script exit when a command fails.
set -e

port="${PORT}"
protocol="${PROTOCOL}"

if [[ -z "${port}" ]]; then
    port=3000 
fi
if [[ -z "${protocol}" ]]; then
    protocol=http 
fi

# The runtime image cannot generate a development certificate the way the SDK image did,
# so an explicit certificate is required when serving https directly.
if [[ "${protocol}" == "https" && -z "${ASPNETCORE_Kestrel__Certificates__Default__Path}" ]]; then
    printf "WARNING: PROTOCOL is https but ASPNETCORE_Kestrel__Certificates__Default__Path is not set.\n"
    printf "         Mount a certificate and set that variable (plus __Password), or terminate TLS at a reverse proxy.\n"
fi

printf "Starting Fenrus at ${protocol}://+:${port}\n"
exec dotnet Fenrus.dll --urls=${protocol}://+:${port}