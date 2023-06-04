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
  rm -rf build
  docker buildx rm fenrusbuilder >/dev/null 2>&1
  exit  
}
get_version() {
  local date=$(date -u -d "+12 hours" +"%y.%m") # NZST date (yy.MM)
  echo "$date"
}

version=$(get_version)

git rev-list --count HEAD > gitversion.txt
docker container stop fenrus  >/dev/null 2>&1
docker container rm fenrus >/dev/null 2>&1
docker buildx rm fenrusbuilder >/dev/null 2>&1

rm -rf build
docker build -t docker-build -f DockerfileBuild . && docker run --rm -v "$(pwd)/build:/build" docker-build
if [ -n "$(find "build" -maxdepth 0 -type d -empty 2>/dev/null)" ]; then
  echo "Build failed"
  exit 1
fi
rm -rf build/runtimes/win*
rm -rf build/runtimes/osx*
rm -rf build/ru-*

docker buildx create --name fenrusbuilder --driver docker-container --platform linux/amd64,linux/arm64
docker buildx use fenrusbuilder
if [ "$1" = "--publish" ]; then
  docker buildx build --push --platform linux/arm64,linux/amd64 \
    -t revenz/fenrus:$version \
    -t revenz/fenrus:latest \
    -t revenz/fenrus \
    -f Dockerfile .

elif [ "$1" = "--dev" ]; then
  docker buildx build --push --platform linux/arm64,linux/amd64 -t revenz/fenrus:develop -f Dockerfile .  
  # docker buildx build --push --platform linux/amd64 -t revenz/fenrus:develop -f Dockerfile .
else
  docker buildx build -t fenrus --platform linux/amd64 -f Dockerfile .
fi

cleanup

if [ "$1" = "--publish" ]; then
  # read -p "Do you want to publish to Dockerhub? (Y/N) " answer
  # echo 'bob'
  # exit
  # if [ "$answer" == "Y" ] || [ "$answer" == "y" ]; then
  #   echo Publishing docker image
  #   docker tag fenrus revenz/fenrus
  #   docker push revenz/fenrus
  #   docker tag fenrus revenz/fenrus:latest
  #   docker push revenz/fenrus:latest    
  #   docker tag fenrus revenz/fenrus:$version
  #   docker push revenz/fenrus:$version
  # else
  #     # do something else
  #   echo Exited without pushing 
  # fi
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