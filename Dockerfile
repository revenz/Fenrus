# Runtime image
#
# The application is compiled separately by DockerfileBuild, which publishes to ./build.
# This image therefore only needs the ASP.NET Core runtime, not the full SDK, which
# drops roughly 600MB and a compiler toolchain out of the shipped image.
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY ./build ./
COPY /Apps /app/Apps
ENV Docker=1
COPY /reset.sh /app/reset.sh
RUN chmod +x /app/reset.sh
COPY /docker-entrypoint.sh /app/docker-entrypoint.sh
RUN chmod +x /app/docker-entrypoint.sh

# make sh open bash
RUN ln -sf /bin/bash /bin/sh

# docker-entrypoint.sh passes --urls explicitly (PORT, default 3000), which overrides
# the ASPNETCORE_HTTP_PORTS=8080 default that .NET 8+ images ship with.
EXPOSE 3000

# The .NET 8+ images include a non-root 'app' user (UID 1654). Running as that user is
# recommended, but existing installs must chown their mounted /app/data first, so it is
# left opt-in rather than flipped for everyone on upgrade.
# USER $APP_UID

ENTRYPOINT ["/app/docker-entrypoint.sh"]
