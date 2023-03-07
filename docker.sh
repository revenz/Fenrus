#!/bin/bash

path="$( cd -- "$(dirname "$0")" >/dev/null 2>&1 ; pwd -P )"
dirData=$path/temp/Data
dirLogs=$path/temp/Logs
echo Data Dir: $dirData
echo Data Dir: $dirLogs

docker container stop fenrus  >/dev/null 2>&1
docker container rm fenrus >/dev/null 2>&1
docker build -t fenrus -f Dockerfile .
docker run -d -p 5000:5000 -v $dirData:/App/Data -v $dirLogs:/App/Logs --restart unless-stopped --name fenrus fenrus

echo -e '\e]8;;http://localhost:5000\a\033[0;32mCtrl+click to open Fenrus\033[0m\e]8;;\a'
