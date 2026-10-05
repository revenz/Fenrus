# ASP.NET Core 10.0 runtime base image
FROM mcr.microsoft.com/dotnet/aspnet:10.0

WORKDIR /app

# Copy application files
COPY ./build ./
COPY /Apps /app/Apps
ENV Docker=1
COPY /reset.sh /app/reset.sh
RUN chmod +x /app/reset.sh
COPY /docker-entrypoint.sh /app/docker-entrypoint.sh
RUN chmod +x /app/docker-entrypoint.sh

# Symlink sh to bash
RUN ln -sf /bin/bash /bin/sh

ENTRYPOINT ["/app/docker-entrypoint.sh"]