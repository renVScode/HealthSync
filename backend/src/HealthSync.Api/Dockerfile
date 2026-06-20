FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY HealthSync.sln ./
COPY src/HealthSync.Api/*.csproj ./src/HealthSync.Api/
COPY src/HealthSync.Core/*.csproj ./src/HealthSync.Core/
COPY src/HealthSync.Infrastructure/*.csproj ./src/HealthSync.Infrastructure
RUN dotnet restore

COPY . .
RUN dotnet publish src/HealthSync.Api/HealthSync.Api.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app .
EXPOSE 5000
ENTRYPOINT ["dotnet", "HealthSync.Api.dll"]
