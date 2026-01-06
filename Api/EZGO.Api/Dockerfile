#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
#WORKDIR /src
COPY ["EZGO.Api/EZGO.Api.csproj", "EZGO.Api/"]
RUN dotnet restore "EZGO.Api/EZGO.Api.csproj"
COPY . .
WORKDIR "/EZGO.Api"
RUN dotnet build "EZGO.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "EZGO.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EZGO.Api.dll"]