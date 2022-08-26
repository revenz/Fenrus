# set the version number
git rev-list --count --first-parent HEAD > buildnum.txt
# docker login
docker buildx build --platform=linux/amd64,linux/arm -t revenz/fenrus:preview --push .
#docker push revenz/fenrus:previewdoc