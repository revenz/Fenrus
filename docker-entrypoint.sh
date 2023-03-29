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

exec dotnet Fenrus.dll --urls=${protocol}://+:${port}