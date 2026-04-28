# ============================================
# Stage 1: Build
# ============================================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files first (layer caching)
COPY PublicConsultation.sln ./
COPY PublicConsultation.Core/PublicConsultation.Core.csproj PublicConsultation.Core/
COPY PublicConsultation.Infrastructure/PublicConsultation.Infrastructure.csproj PublicConsultation.Infrastructure/
COPY PublicConsultation.BlazorServer/PublicConsultation.BlazorServer.csproj PublicConsultation.BlazorServer/

# Restore dependencies
RUN dotnet restore PublicConsultation.BlazorServer/PublicConsultation.BlazorServer.csproj

# Copy everything else
COPY PublicConsultation.Core/ PublicConsultation.Core/
COPY PublicConsultation.Infrastructure/ PublicConsultation.Infrastructure/
COPY PublicConsultation.BlazorServer/ PublicConsultation.BlazorServer/

# Build and publish
RUN dotnet publish PublicConsultation.BlazorServer/PublicConsultation.BlazorServer.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# ============================================
# Stage 2: Runtime
# ============================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Install ICU for globalization support
RUN apt-get update && apt-get install -y --no-install-recommends \
    libicu-dev \
    && rm -rf /var/lib/apt/lists/*

# Copy published output
COPY --from=build /app/publish .

# Environment defaults
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080

# Health check
HEALTHCHECK --interval=30s --timeout=10s --retries=3 \
    CMD curl -f http://localhost:8080/ || exit 1

ENTRYPOINT ["dotnet", "PublicConsultation.BlazorServer.dll"]
