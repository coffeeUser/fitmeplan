ARG NODE_IMAGE=node:11
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1.0-alpine3.10 AS base
WORKDIR /app
EXPOSE 5002

FROM mcr.microsoft.com/dotnet/core/sdk:3.1.100-alpine3.10 AS dotnet-build

# This defines the `ARG` inside the build-stage (it will be executed after `FROM`
# in the child image, so it's a new build-stage). Don't set a default value so that
# the value is set to what's currently set for `BUILD_VERSION`
ONBUILD ARG BUILD_VERSION

# If BUILD_VERSION is set/non-empty, use it, otherwise use a default value
ONBUILD ARG VERSION=${BUILD_VERSION:-1.0.0}

WORKDIR /src

FROM ${NODE_IMAGE} as node-build
WORKDIR /web
COPY Fitmeplan.Client .
WORKDIR /web/ClientApp
RUN npm install
RUN npm run-script build

FROM dotnet-build as publish
WORKDIR /src/Fitmeplan.Client/wwwroot
COPY --from=node-build /web/wwwroot .
WORKDIR /src
COPY . .
WORKDIR /src/Fitmeplan.Client
RUN dotnet publish /p:Version=$VERSION -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Fitmeplan.Client.dll"]