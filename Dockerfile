FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
WORKDIR /app

# Copy everything
COPY . ./
# sets the version
RUN chmod +x ./setversion.sh && ./setversion.sh
# Restore as distinct layers
RUN dotnet restore
# Build and publish a release
RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY --from=build-env /app/out .
COPY /Apps /app/Apps
ENV Docker=1
COPY /reset.sh /app/reset.sh
RUN chmod +x /app/reset.sh

# make sh open bash
RUN ln -sf /bin/bash /bin/sh
    
ENTRYPOINT ["dotnet", "Fenrus.dll", "--urls", "http://+:3000"]