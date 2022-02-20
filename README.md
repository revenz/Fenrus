![icon_128](https://user-images.githubusercontent.com/958400/154829266-62206846-c6ef-4718-9910-2b83eb6aa41c.png)

# Fenrus


Fenrus is a magical gateway to the wonderful wizardly world.   

It acts as a magic home page and allows for a quick overview of your favourite apps/sites and lets you organize and quickly navigate this magical world.

---
![fenrus_01](https://user-images.githubusercontent.com/958400/154829573-15cc1170-561f-433a-92fc-d0d9daa85902.png)

---

## Installation

### Node
Fenrus is a Node application and requires NodeJS to run.  Once NodeJS is installed you can run Fenrus 
> node app.js

### Docker
Docker is the preferred method of installing Fenrus
```
docker run -d \
-name=Fenrus\
-p 3000:3000 \
-v /path/to/data:/app/data\
-v /path/to/logging:/app/wwwroot/images \
-v /path/to/temp:/temp \
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
      - /path/to/logs:/app/wwwroot/images
    ports:
      - 3000:3000
    restart: unless-stopped
```
Fenrus will save all the user confiuration data in the folder /app/data so map this folder outside the docker container.  
Also it will store custom images under /app/wwwroot/images, so map this folder outside the docker image aswell.

---

## Getting Started

First you need to register a user, you can do this on the login page by entering a username and password and clicking "Register" if no user with that username exists, a new one will be created.  

![fenrus_login](https://user-images.githubusercontent.com/958400/154829712-5b7dde64-eb4b-4e1d-9991-29d160d4b057.png)


### Admin
The first user created in the system will automically be assigned the admin role.
This role allows the user to manage other users.

---

## Configuration

### Groups
Groups contain applications and links.  The Width/Height settings are used by some Themes.   The "Default" theme will use a unit of 1 for the height.   This unit of 1 is size of a "Small" dashboard item.   So in the "Media" group shown in below, the height is set to 4, as it is 4x small items high.  Width is not used in the default theme, it is however used the basic theme.

![fenrus_media](https://user-images.githubusercontent.com/958400/154829815-bcb20f43-35bb-4550-a955-319d9216f2be.png)

### Group Items
Shortcuts are broken down into 3 types

#### Links
These are basic links to websites, either internal or external, and show no extra information.  You can configure an icon for these links, or if left blank Fenrus will try to magically download the favicon for the site.

#### Apps - Basic
These are a step above links, they are known to Fenrus, and will have a high-resolution icon, perhaps a default URL, but little else.

#### Apps - Smart
This is where the magic really happens.  These smart apps, or spell casts if you will, have extra information that Fenrus can download and query to display more information about the app.
This could be as simple as some basic information, or it could be a feature-rich magical experience.

---

## FAQ

Q: What's with all the magic stuff?
A: I named the app Fenrus, Erasmus was taken, from a wizard character from Quest for Glory.  It's hard coming up with a domain, especially for something so generic as a homepage-type app.


Q: Why is the logo a wolf when it's named after a magic rat?
A: I googled Fenrus, found it's a giant wolf in World of Warcraft, I couldn't find a decent magic icon so I went for the Wolf for now.

---

## TODO
- Implement a application update system, so user do not need to install a new version of Fenrus to get the latest applications
- Document how to write a application.  These are written in Javascript, and are not complex to write, however there are some gotcha's that I need to document.
