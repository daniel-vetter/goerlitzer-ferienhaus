FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /source
COPY src/WebApp .
RUN dotnet publish -c release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:5.0
WORKDIR /app
COPY --from=build /app ./
ENTRYPOINT ["dotnet", "WebApp.dll"]