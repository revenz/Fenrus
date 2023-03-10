#!/bin/bash

hyperlink(){ 
  printf '\033[0;32m'
  printf "$1"
  printf ':\033[0m '; 
  printf '\e]8;;%s\e\\%s\e]8;;\e\\' "$2" "${3:-$2}";
  echo
}

docker container stop fenrus  >/dev/null 2>&1
docker container rm fenrus >/dev/null 2>&1
docker build -t fenrus -f Dockerfile .

if [ "$1" = "--publish" ]; then
  echo Publishing docker image
  docker tag fenrus revenz/fenrus:dotnet
  docker push revenz/fenrus:dotnet
else
  echo Running docker image
  
  path="$( cd -- "$(dirname "$0")" >/dev/null 2>&1 ; pwd -P )"
  dirData=$path/temp/data
  mkdir -p $dirData
  dirLogs=$path/temp/data/logs
  mkdir -p $dirLogs
  
  docker run -d -p 3000:3000 -v $dirData:/app/data --restart unless-stopped --name fenrus fenrus 
  
  hyperlink 'Data Directory' file://$dirData $dirData
  hyperlink 'Logs Directory' file://$dirLogs $dirLogs 
  hyperlink 'Fenrus App URL' http://localhost:3000 http://localhost:3000
fi