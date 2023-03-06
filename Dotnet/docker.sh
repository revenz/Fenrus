# !/bin/bash

docker build -t fenrus -f Dockerfile .
docker container stop fenrus
docker container rm fenrus
# docker create --name fenrus fenrus
docker run -d -p 5000:5000 --name fenrus fenrus