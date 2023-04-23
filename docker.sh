#!/bin/bash

hyperlink(){ 
  printf '\033[0;32m'
  printf "$1"
  printf ':\033[0m '; 
  printf '\e]8;;%s\e\\%s\e]8;;\e\\' "$2" "${3:-$2}";
  echo
}

git rev-list --count HEAD > gitversion.txt
docker container stop fenrus  >/dev/null 2>&1
docker container rm fenrus >/dev/null 2>&1
docker build -t fenrus -f Dockerfile .

if [ "$1" = "--publish" ]; then
  read -p "Do you want to publish to Dockerhub? (Y/N) " answer
  if [ "$answer" == "Y" ] || [ "$answer" == "y" ]; then
    echo Publishing docker image
    docker tag fenrus revenz/fenrus:dotnet
    docker push revenz/fenrus:dotnet
  else
      # do something else
    echo Exited without pushing 
  fi
elif [ "$1" = "--dev" ]; then
  echo Publishing develop docker image
  docker tag fenrus revenz/fenrus:develop
  docker push revenz/fenrus:develop
else
  echo Running docker image
  
  path="$( cd -- "$(dirname "$0")" >/dev/null 2>&1 ; pwd -P )"
  dirData=$path/temp/data
  mkdir -p $dirData
  dirLogs=$path/temp/data/logs
  mkdir -p $dirLogs
  
  port=3000
  
  if [ "$1" = "--port" ]; then
    port="$2"
  fi
  
  docker run -d -p $port:3000 -v $dirData:/app/data -e puid=1000 -e pgid=1000 --restart unless-stopped --name fenrus fenrus 
  
  hyperlink 'Data Directory' file://$dirData $dirData
  hyperlink 'Logs Directory' file://$dirLogs $dirLogs 
  hyperlink 'Fenrus App URL' http://localhost:$port http://localhost:$port
fi