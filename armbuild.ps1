# docker login
docker buildx build --platform=linux/arm . -t revenz/nodeportal:arm
docker push revenz/nodeportal:arm