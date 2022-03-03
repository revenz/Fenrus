FROM alpine:3.15

# Create app directory
WORKDIR /app

# Bundle app source
COPY . .


RUN apk add --update --no-cache \
    make \
    g++ \
    jpeg-dev \
    cairo-dev \
    giflib-dev \
    pango-dev \
    libtool \
    freetype-dev \
    autoconf \
    automake

RUN apk -U --update --no-cache add --virtual=build-dependencies \
      npm && \
    apk -U --update --no-cache add \
      nodejs && \    
    rm -rf ./.git && \
    npm install && \
    npm ci --only=production && \
    mkdir -p ./data && \
    apk del --purge \
      build-dependencies

EXPOSE 3000
CMD [ "node", "app.js" ]
