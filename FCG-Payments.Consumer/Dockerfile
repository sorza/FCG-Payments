ARG BUILD_VERSION=1.0.0
ARG BUILD_DATE
ARG VCS_REF

FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /src

COPY ["FCG-Payments.Consumer/FCG-Payments.Consumer.csproj", "FCG-Payments.Consumer/"]
COPY ["FCG-Payments.Infrastructure/FCG-Payments.Infrastructure.csproj", "FCG-Payments.Infrastructure/"]
COPY ["FCG-Payments.Application/FCG-Payments.Application.csproj", "FCG-Payments.Application/"]
COPY ["FCG-Payments.Domain/FCG-Payments.Domain.csproj", "FCG-Payments.Domain/"]

RUN dotnet restore "FCG-Payments.Consumer/FCG-Payments.Consumer.csproj"

COPY . .
WORKDIR /src/FCG-Payments.Consumer

RUN dotnet publish "FCG-Payments.Consumer.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false \
    /p:InvariantGlobalization=true \
    --no-restore

FROM mcr.microsoft.com/dotnet/runtime:8.0-alpine AS runtime

LABEL maintainer="FCG Team" \
      version="${BUILD_VERSION}" \
      description="FCG Payments Consumer - Event processor for payments domain" \
      org.opencontainers.image.created="${BUILD_DATE}" \
      org.opencontainers.image.revision="${VCS_REF}" \
      org.opencontainers.image.source="https://github.com/alexandresorza/fcg-microservices"

WORKDIR /app

RUN addgroup -g 1000 appuser && \
    adduser -u 1000 -G appuser -s /bin/sh -D appuser && \
    chown -R appuser:appuser /app

COPY --from=build --chown=appuser:appuser /app/publish .

USER appuser

ENV DOTNET_RUNNING_IN_CONTAINER=true \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=true

ENTRYPOINT ["dotnet", "FCG-Payments.Consumer.dll"]
