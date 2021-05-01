FROM mcr.microsoft.com/dotnet/core/aspnet:3.1.0-alpine3.10 AS base

FROM Iam/dotnet-publish AS publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/web/Fitmeplan.Api/bin/Release/netcoreapp3.1/publish .
ENTRYPOINT ["dotnet", "Fitmeplan.Api.dll"]
