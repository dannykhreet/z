#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["EZGO.CLI.MigrationService/EZGO.CLI.MigrationService.csproj", "EZGO.CLI.MigrationService/"]
COPY ["EZGO.Api.Interfaces/EZGO.Api.Interfaces.csproj", "EZGO.Api.Interfaces/"]
COPY ["EZGO.Api.Models/EZGO.Api.Models.csproj", "EZGO.Api.Models/"]
RUN dotnet restore "EZGO.CLI.MigrationService/EZGO.CLI.MigrationService.csproj"
COPY . .
WORKDIR "/src/EZGO.CLI.MigrationService"
RUN dotnet build "EZGO.CLI.MigrationService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "EZGO.CLI.MigrationService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EZGO.CLI.MigrationService.dll"]