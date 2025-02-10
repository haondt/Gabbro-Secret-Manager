#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER app
WORKDIR /app
EXPOSE 8080

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
ARG TARGETARCH
WORKDIR /src
COPY ["GabbroSecretManager/GabbroSecretManager.csproj", "GabbroSecretManager/"]
COPY ["nuget.config", "."]
RUN dotnet restore "./GabbroSecretManager/GabbroSecretManager.csproj" --arch $TARGETARCH
COPY . .
WORKDIR "/src/GabbroSecretManager"
RUN dotnet build "./GabbroSecretManager.csproj" -c $BUILD_CONFIGURATION -o /app/build --arch $TARGETARCH

FROM --platform=$BUILDPLATFORM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./GabbroSecretManager.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false --arch $TARGETARCH

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GabbroSecretManager.dll"]

