# !/bin/bash

path="$( cd -- "$(dirname "$0")" >/dev/null 2>&1 ; pwd -P )"
dirData=$path/temp/Data
dirLogs=$path/temp/Logs
echo Data Dir: $dirData
echo Data Dir: $dirLogs

docker container stop fenrus  >/dev/null 2>&1
docker container rm fenrus >/dev/null 2>&1
docker build -t fenrus -f Dockerfile .
docker run -d -p 5000:5000 -v $dirData:/App/Data -v $dirLogs:/App/Logs --restart unless-stopped --name fenrus fenrus