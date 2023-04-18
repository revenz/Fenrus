<img src="wwwroot/fenrus.svg" width="170" height="170">

# Fenrus


Fenrus personal home page/dashboard.  

It allows you to have a custom home page/new tab page with quick access to your personal apps.

For support use our [Discord Server](https://discord.gg/xbYK8wFMeU)

[![Donate](https://img.shields.io/badge/Donate-Patreon-blue.svg)](https://www.patreon.com/revenz)
[![Donate](https://img.shields.io/badge/Donate-PayPal-green.svg)](https://www.paypal.com/donate/?hosted_button_id=ZJLFMQSQ6WX3J)

---

![image](https://user-images.githubusercontent.com/958400/232895057-f40073d9-5d4a-4324-9ba9-7e837fca7df6.png)

---

## Installation

### Dotnet
Fenrus is a Dotnet application and requires [ASP.NET Core Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/6.0) to run.  

Then you can run Fenrus
```
dotnet Fenrus.dll
```
Or to specify a port

```
dotnet Fenrus.dll --urls=http://*:1234
```

### Docker
Docker is the preferred method of installing Fenrus
```
docker run -d \
--name=Fenrus \
-e TZ=Pacific/Auckland \
-p 3000:3000 \
-v /path/to/data:/app/data \
--restart unless-stopped \
revenz/fenrus:latest
```

Note: You can customise the port used by using the environmental variable "Port"
```
-e Port=1234
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
    ports:
      - 3000:3000
    restart: unless-stopped
```
Fenrus will save all the user configuration data in the folder /app/data so map this folder outside the docker container.  

All the configuration is saved into LiteDB file, Fenrus.db.   There is an encryption key alongside this database file used to encrypt sensitive data inside the database.  This way if you need to provide the database for support reasons, your sensitive data cannot be read.

---

## Getting Started

First, you need to register a user, you can do this on the login page by entering a username and password and clicking "Register" if no user with that username exists, a new one will be created.

![image](https://user-images.githubusercontent.com/958400/232894085-4d7d90c5-bf23-4e9a-9262-27b3cec5022b.png)

### Admin
The first user created in the system will automatically be assigned the admin role.
This role allows the user to manage other users.

---

## Configuration

### Groups
Groups contain applications, links and other dashboards.   

![image](https://user-images.githubusercontent.com/958400/232895199-a4dc5920-8a97-4faa-a842-d950a3834553.png)


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


---

### Search Engines
```
Name: The name of the search engine
URL: The URL for the search query with %s being repalced by the search term
Shortcut: The shortcut to type to use this search engine (if not the default)
Icon: The icon to show when using this search engine
```

![image](https://user-images.githubusercontent.com/958400/232895446-9ad716b4-ff22-4431-a21a-9cd809663944.png)


---

## FAQ


Q: What is the default username/password?

A: There is no default user.  Enter a username and password, then click the "Register" button.  This will create a new administrator user.   You can then go ot "System" and turn off registrations if you do not wish to allow open registrations.

--- 

Q: Why was Fenrus rewritten in .NET?

A: Fenrus was original a much smaller application and was written in NodeJS.  This was simple when Fenrus was a simple dashboard application.  But as features were added (Docker Terminals/Logs, SSH, Up-Time Recording), the app become more complicated.   To ease development, it was rewritten in  .NET using Blazor Server Side.  This drastically simplifies the code for system configuration/settings, and makes it easier to add more features.
.NET has the added benefit of being a faster rendering engine than Node, resulting in faster page loading.
