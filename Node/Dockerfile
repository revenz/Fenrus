FROM alpine:3.15

# Create app directory
WORKDIR /app

# Bundle app source
COPY apps/ apps/
COPY helpers/ helpers/
COPY middleware/ middleware/
COPY models/ models/
COPY node_modules/ node_modules/
COPY routes/ routes/
COPY services/ services/
COPY strategies/ strategies/
COPY views/ views/
COPY wwwroot/ wwwroot/
COPY app.js app.js
COPY buildnum.txt buildnum.txt
COPY defaultconfig.json defaultconfig.json
COPY Globals.js Globals.js
COPY package-lock.json package-lock.json
COPY package.json package.json


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
      bash \
      nodejs \
      cairo \
      jpeg \
      pango \
      giflib \
      msttcorefonts-installer \
      fontconfig && \
      update-ms-fonts && \
      fc-cache -f &&\
      rm -rf ./.git && \
      npm install && \
      npm ci --only=production && \
      mkdir -p ./data && \
      # Removal build-dependencies
      apk del --purge build-dependencies

EXPOSE 3000
CMD [ "node", "app.js" ]
