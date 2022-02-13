FROM node:16

# Create app directory
WORKDIR /app

# Install app dependencies
# A wildcard is used to ensure both package.json AND package-lock.json are copied
# where available (npm@5+)
COPY package*.json ./

RUN npm install

RUN npm ci --only=production

# Bundle app source
COPY . .

EXPOSE 80
CMD [ "node", "app.js" ]