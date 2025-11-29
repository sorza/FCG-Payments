FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ../FCG-Payments.Api/FCG-Payments.Api.csproj ./FCG-Payments.Api/
COPY ../FCG-Payments.Domain/FCG-Payments.Domain.csproj ./FCG-Payments.Domain/
COPY ../FCG-Payments.Application/FCG-Payments.Application.csproj ./FCG-Payments.Application/
COPY ../FCG-Payments.Infrastructure/FCG-Payments.Infrastructure.csproj ./FCG-Payments.Infrastructure/

RUN dotnet restore ./FCG-Payments.Api/FCG-Payments.Api.csproj

COPY .. .

RUN dotnet publish ./FCG-Payments.Api/FCG-Payments.Api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "FCG-Payments.Api.dll"]
