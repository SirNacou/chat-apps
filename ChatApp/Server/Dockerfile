# --- STAGE 1: BASE IMAGE FOR THE API SERVER ---
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER root
RUN apt-get update && apt-get install -y curl --no-install-recommends && rm -rf /var/lib/apt/lists/*
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# --- STAGE 2: BUILD & BUNDLE GENERATION ---
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS restore
WORKDIR /src
COPY ["Server/Server.csproj", "Server/"]
RUN dotnet restore "Server/Server.csproj"
COPY . .

FROM restore AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR "/src/Server"
RUN dotnet build "./Server.csproj" -c $BUILD_CONFIGURATION -o /app/build --no-restore


FROM restore AS bundle
WORKDIR "/src/Server"
# 1. Install Entity Framework Core CLI tools inside the build container
RUN dotnet tool install --global dotnet-ef
ENV PATH="$PATH:/root/.dotnet/tools"

# 2. Compile your migrations into a single, standalone Linux executable binary
RUN dotnet ef migrations bundle --self-contained -r linux-x64 -o /app/efbundle

# --- STAGE 3: PUBLISH THE API ---
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Server.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# ─── TARGET 1: THE RUNTIME WEB API SERVER ───
FROM base AS server
WORKDIR /app

# 1. Temporarily switch to root to fix directory permissions
USER root

# 2. Pre-create the keys directory and grant ownership to the non-root app user
RUN mkdir -p /home/app/.aspnet/DataProtection-Keys && \
    chown -R $APP_UID:$APP_UID /home/app/.aspnet

# 3. Drop privileges back to the secure app user
USER $APP_UID

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Server.dll"]

# ─── TARGET 2: THE RUNTIME MIGRATION RUNNER ───
FROM mcr.microsoft.com/dotnet/runtime:10.0 AS migration
WORKDIR /app
# Copy the compiled executable from the build stage
COPY --from=bundle /app/efbundle .
# Ensure the binary has explicit execution permissions inside Linux
USER root
RUN chmod +x ./efbundle
USER $APP_UID
# The bundle automatically reads 'ConnectionStrings__DefaultConnection' from the environment
ENTRYPOINT ["./efbundle"]
