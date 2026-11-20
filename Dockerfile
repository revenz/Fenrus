# ASP.NET Core 10.0 runtime base image
FROM mcr.microsoft.com/dotnet/aspnet:10.0

# what the image is; Fortilla (and other docker tools) show its version from it, else the base image's
ARG BUILD_VERSION=0.0.0.0
LABEL org.opencontainers.image.title="Fenrus" \
      org.opencontainers.image.description="Fenrus: your personal home page and dashboard" \
      org.opencontainers.image.version="${BUILD_VERSION}"

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