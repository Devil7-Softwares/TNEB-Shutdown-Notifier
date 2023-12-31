#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base

WORKDIR /app/x64
RUN apt-get update && apt-get install -y --allow-unauthenticated libleptonica-dev libtesseract-dev && rm -rf /var/lib/apt/lists/*
RUN ln -s /usr/lib/x86_64-linux-gnu/liblept.so.5 /app/x64/libleptonica-1.82.0.so
RUN ln -s /usr/lib/x86_64-linux-gnu/libtesseract.so.5 /app/x64/libtesseract50.so
RUN ln -s /usr/lib/x86_64-linux-gnu/libdl.so.2 /usr/lib/x86_64-linux-gnu/libdl.so

USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["TNEB.Shutdown.Notifier.Web/TNEB.Shutdown.Notifier.Web.csproj", "TNEB.Shutdown.Notifier.Web/"]
RUN dotnet restore "./TNEB.Shutdown.Notifier.Web/./TNEB.Shutdown.Notifier.Web.csproj"
COPY . .
WORKDIR "/src/TNEB.Shutdown.Notifier.Web"
RUN dotnet build "./TNEB.Shutdown.Notifier.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./TNEB.Shutdown.Notifier.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TNEB.Shutdown.Notifier.Web.dll"]