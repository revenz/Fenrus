![icon_128](https://user-images.githubusercontent.com/958400/154829266-62206846-c6ef-4718-9910-2b83eb6aa41c.png)

# Fenrus


Fenrus personal home page/dasbhoard.  

It allows you to have a custom home page/new tab page with quick access to your personal apps.

For support use our [Discord Server](https://discord.gg/xbYK8wFMeU)

[![Donate](https://img.shields.io/badge/Donate-Patreon-blue.svg)](https://www.patreon.com/revenz)
[![Donate](https://img.shields.io/badge/Donate-PayPal-green.svg)](https://www.paypal.com/donate/?hosted_button_id=ZJLFMQSQ6WX3J)

---
![image](https://user-images.githubusercontent.com/958400/157801403-64cc7b99-f81d-41b6-85f7-47546bd904a9.png)

---

## Installation

### Node
Fenrus is a Node application and requires NodeJS to run.  
Once NodeJS is installed you need to download the packages Fenrus uses
```
npm install
```

Then you can run Fenrus 
```
node app.js
```

### Docker
Docker is the preferred method of installing Fenrus
```
docker run -d \
-name=Fenrus\
-p 3000:3000 \
-v /path/to/data:/app/data\
-v /path/to/images:/app/wwwroot/images
--restart unless-stopped \
revenz/fenrus:latest
```
```
services:
  fenrus:
    image: revenz/fenrus
    container_name: fenrus
    environment:
      - TZ=Pacific/Auckland
    volumes:
      - /path/to/data:/app/data
      - /path/to/images:/app/wwwroot/images
    ports:
      - 3000:3000
    restart: unless-stopped
```
Fenrus will save all the user configuration data in the folder /app/data so map this folder outside the docker container.  
Also, it will store custom images under /app/wwwroot/images, so map this folder outside the docker image as well.

---

## Getting Started

First, you need to register a user, you can do this on the login page by entering a username and password and clicking "Register" if no user with that username exists, a new one will be created.  

![image](https://user-images.githubusercontent.com/958400/157801936-24eca81b-1ee1-4f28-976c-c9e1ae54f1af.png)


### Admin
The first user created in the system will automatically be assigned the admin role.
This role allows the user to manage other users.

---

## Configuration

### Groups
Groups contain applications, links and other dashboards.   

![image](https://user-images.githubusercontent.com/958400/157801996-e94c3406-ff6b-43a2-acfe-7fb6a174b822.png)

### Group Items
Shortcuts are broken down into 4 types

#### Links
These are basic links to websites, either internal or external, and show no extra information.  You can configure an icon for these links, or if left blank Fenrus will try to magically download the favicon for the site.

#### Apps - Basic
These are a step above links, they are known to Fenrus, and will have a high-resolution icon, perhaps a default URL, but little else.

#### Apps - Smart
This is where the magic really happens.  These smart apps, or spell casts if you will, have extra information that Fenrus can download and query to display more information about the app.
This could be as simple as some basic information, or it could be a feature-rich magical experience.

#### Dashboards
These link to other dashboards.   You can add a dashboard link to a group or they are also available through the drop down menu at the top of the page.
![image](https://user-images.githubusercontent.com/958400/157802115-e1eed5f1-ef39-46f5-a719-69e01e0c6df3.png)


---

### Search Engines
```
Name: The name of the search engine
URL: The URL for the search query with %s being repalced by the search term
Shortcut: The shortcut to type to use this search engine (if not the default)
Icon: The icon to show when using this search engine
```
![image](https://user-images.githubusercontent.com/958400/157802216-44e9f2cf-e874-493c-9d46-3124f771e2af.png)


## FAQ


Q: What is the default username/password?

A: There is no default user.  Enter a username and password, then click the "Register" button.  This will create a new administrator user.   You can then go ot "System" and turn off registrations if you do not wish to allow open registrations.
