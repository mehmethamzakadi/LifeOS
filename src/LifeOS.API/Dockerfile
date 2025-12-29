# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
# ✅ Merkezi paket versiyon yönetimi için gerekli dosyalar
COPY ["Directory.Build.props", "./"]
COPY ["Directory.Packages.props", "./"]
# Proje dosyalarını kopyala
COPY ["src/LifeOS.API/LifeOS.API.csproj", "src/LifeOS.API/"]
COPY ["src/LifeOS.Infrastructure/LifeOS.Infrastructure.csproj", "src/LifeOS.Infrastructure/"]
COPY ["src/LifeOS.Application/LifeOS.Application.csproj", "src/LifeOS.Application/"]
COPY ["src/LifeOS.Domain/LifeOS.Domain.csproj", "src/LifeOS.Domain/"]
COPY ["src/LifeOS.Persistence/LifeOS.Persistence.csproj", "src/LifeOS.Persistence/"]
RUN dotnet restore "./src/LifeOS.API/LifeOS.API.csproj"
COPY . .
WORKDIR "/src/src/LifeOS.API"
RUN dotnet build "./LifeOS.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./LifeOS.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .



# --- EKLEMEN GEREKEN KISIM BAŞLANGIÇ ---
# Root kullanıcısına geçici olarak geçip klasörü hazırlarız
USER root
# Klasörü oluştur ve sahipliğini .NET'in kullandığı kullanıcıya (app) ver
RUN mkdir -p /app/wwwroot/uploads && \
    chown -R app:app /app/wwwroot/uploads

# Tekrar güvenli kullanıcıya dön
USER app
# --- EKLEMEN GEREKEN KISIM BİTİŞ ---


ENTRYPOINT ["dotnet", "LifeOS.API.dll"]