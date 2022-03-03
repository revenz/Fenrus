FROM alpine:3.15

# Create app directory
WORKDIR /app

# Bundle app source
COPY . .

#RUN apk add --no-cache \
#  build-base \
#  g++ \
#  cairo-dev \
#  jpeg-dev \
#  pango-dev \
#  giflib-dev

#RUN apk --no-cache add msttcorefonts-installer fontconfig && \
#    update-ms-fonts && \
#    fc-cache -f

# Add what to need for build/compose deps to build-dependencies
RUN apk -U --update --no-cache add --virtual=build-dependencies \
      npm \
      build-base \
      g++ \
      cairo-dev \
      jpeg-dev \
      pango-dev \
      giflib-dev && \
      # Runtime deps
      apk -U --update --no-cache add \
      nodejs \
      cairo \
      jpeg \
      pango \
      giflib \
      msttcorefonts-installer \
      fontconfig && \
      fc-cache -f &&\
      rm -rf ./.git && \
      npm install && \
      npm ci --only=production && \
      mkdir -p ./data && \
      # Removal build-dependencies
      apk del --purge build-dependencies

EXPOSE 3000
CMD [ "node", "app.js" ]
