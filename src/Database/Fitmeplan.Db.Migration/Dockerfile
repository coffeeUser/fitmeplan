FROM mcr.microsoft.com/dotnet/core/runtime:3.1.0-bionic AS base
WORKDIR /app

# apt-get and system utilities
RUN apt-get update && apt-get install -y \
	curl apt-transport-https debconf-utils \
    && rm -rf /var/lib/apt/lists/*
# adding custom MS repository
RUN apt-get update && apt-get install -y gnupg2
RUN curl https://packages.microsoft.com/keys/microsoft.asc | apt-key add -
RUN curl https://packages.microsoft.com/config/ubuntu/18.04/prod.list > /etc/apt/sources.list.d/mssql-release.list
# install SQL Server drivers and tools
RUN apt-get update && ACCEPT_EULA=Y apt-get install -y mssql-tools unixodbc-dev
RUN echo 'export PATH="$PATH:/opt/mssql-tools/bin"' >> ~/.bashrc
RUN /bin/bash -c "source ~/.bashrc"
#install locales
RUN apt-get -y install locales
RUN locale-gen en_US.UTF-8
RUN update-locale LANG=en_US.UTF-8

FROM mcr.microsoft.com/dotnet/core/sdk:3.1.100-bionic AS build
WORKDIR /src
COPY ["src/Database/Fitmeplan.Db.Migration/Fitmeplan.Db.Migration.csproj", "src/Fitmeplan.Db.Migration/"]
RUN dotnet restore "src/Database/Fitmeplan.Db.Migration/Fitmeplan.Db.Migration.csproj"
COPY . .
WORKDIR "/src/src/Fitmeplan.Db.Migration"
RUN dotnet build "Fitmeplan.Db.Migration.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "Fitmeplan.Db.Migration.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Fitmeplan.Db.Migration.dll"]
