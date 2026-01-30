ARG BUILD_VERSION=1.0.0
ARG BUILD_DATE
ARG VCS_REF

FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /src

COPY ["FCG-Payments.Api/FCG-Payments.Api.csproj", "FCG-Payments.Api/"]
COPY ["FCG-Payments.Application/FCG-Payments.Application.csproj", "FCG-Payments.Application/"]
COPY ["FCG-Payments.Domain/FCG-Payments.Domain.csproj", "FCG-Payments.Domain/"]
COPY ["FCG-Payments.Infrastructure/FCG-Payments.Infrastructure.csproj", "FCG-Payments.Infrastructure/"]

RUN dotnet restore "FCG-Payments.Api/FCG-Payments.Api.csproj"

COPY . .
WORKDIR /src/FCG-Payments.Api

RUN dotnet publish "FCG-Payments.Api.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false \
    /p:InvariantGlobalization=true \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS runtime

LABEL maintainer="FCG Team" \
      version="${BUILD_VERSION}" \
      description="FCG Payments API - Microservice for payment processing" \
      org.opencontainers.image.created="${BUILD_DATE}" \
      org.opencontainers.image.revision="${VCS_REF}" \
      org.opencontainers.image.source="https://github.com/alexandresorza/fcg-microservices"

WORKDIR /app

RUN apk add --no-cache icu-libs && \
    addgroup -g 1000 appuser && \
    adduser -u 1000 -G appuser -s /bin/sh -D appuser && \
    chown -R appuser:appuser /app

COPY --from=build --chown=appuser:appuser /app/publish .

USER appuser

ENV ASPNETCORE_URLS="http://+:8080" \
    DOTNET_RUNNING_IN_CONTAINER=true \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

EXPOSE 8080

HEALTHCHECK --interval=30s --timeout=3s --start-period=40s --retries=3 \
    CMD wget --no-verbose --tries=1 --spider http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "FCG-Payments.Api.dll"]
