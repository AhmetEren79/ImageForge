# Dockerfile — ImageForge .NET API multi-stage build.
# Build aşaması: restore → build → publish
# Runtime aşaması: minimal ASP.NET Core runtime image

FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS base
WORKDIR /app
EXPOSE 5000
RUN apt-get update && apt-get install -y --no-install-recommends curl && rm -rf /var/lib/apt/lists/*

FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

# Proje dosyalarını kopyala ve restore et (layer caching)
COPY ["src/ImageForge.Api/ImageForge.Api.csproj", "ImageForge.Api/"]
COPY ["src/ImageForge.Infrastructure/ImageForge.Infrastructure.csproj", "ImageForge.Infrastructure/"]
COPY ["src/ImageForge.Application/ImageForge.Application.csproj", "ImageForge.Application/"]
COPY ["src/ImageForge.Domain/ImageForge.Domain.csproj", "ImageForge.Domain/"]
RUN dotnet restore "ImageForge.Api/ImageForge.Api.csproj"

# Tüm kaynak kodunu kopyala ve build et
COPY src/ .
RUN dotnet build "ImageForge.Api/ImageForge.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ImageForge.Api/ImageForge.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS=http://+:5000
ENTRYPOINT ["dotnet", "ImageForge.Api.dll"]
