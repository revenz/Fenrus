#!/bin/bash

# note: need to run the following to install dockerx for building both x64 and arm64
# docker buildx install

hyperlink(){ 
  printf '\033[0;32m'
  printf "$1"
  printf ':\033[0m '; 
  printf '\e]8;;%s\e\\%s\e]8;;\e\\' "$2" "${3:-$2}";
  echo
}

cleanup() {
  docker buildx rm fenrusbuilder >/dev/null 2>&1
  exit  
}

git rev-list --count HEAD > gitversion.txt
docker container stop fenrus  >/dev/null 2>&1
docker container rm fenrus >/dev/null 2>&1
docker buildx rm fenrusbuilder >/dev/null 2>&1

docker buildx create --name fenrusbuilder --driver docker-container --platform linux/amd64,linux/arm64
docker buildx use fenrusbuilder
if [ "$1" = "--publish" ]; then
  docker buildx build --push --platform linux/arm64,linux/amd64 -t revenz/fenrus:dotnet -f Dockerfile .
  cleanup
elif [ "$1" = "--dev" ]; then
  docker buildx build --push --platform linux/arm64,linux/amd64 -t revenz/fenrus:develop -f Dockerfile .  
  cleanup
else
  docker buildx build -t fenrus --platform linux/amd64 -f Dockerfile .
fi

docker buildx rm fenrusbuilder
# docker build -t fenrus -f Dockerfile .

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