#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["EZGO.WorkerService.Exporter/EZGO.WorkerService.Exporter.csproj", "EZGO.WorkerService.Exporter/"]
RUN dotnet restore "EZGO.WorkerService.Exporter/EZGO.WorkerService.Exporter.csproj"
COPY . .
WORKDIR "/src/EZGO.WorkerService.Exporter"
RUN dotnet build "EZGO.WorkerService.Exporter.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "EZGO.WorkerService.Exporter.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EZGO.WorkerService.Exporter.dll"]