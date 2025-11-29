FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ../FCG-Payments-WorkerService/FCG-Payments-WorkerService.csproj ./FCG-Payments-WorkerService/
COPY ../FCG-Payments.Domain/FCG-Payments.Domain.csproj ./FCG-Payments.Domain/
COPY ../FCG-Payments.Application/FCG-Payments.Application.csproj ./FCG-Payments.Application/
COPY ../FCG-Payments.Infrastructure/FCG-Payments.Infrastructure.csproj ./FCG-Payments.Infrastructure/

RUN dotnet restore ./FCG-Payments-WorkerService/FCG-Payments-WorkerService.csproj

# Copia todo o código
COPY . .

RUN dotnet publish ./FCG-Payments-WorkerService/FCG-Payments-WorkerService.csproj -c Release -o /app/publish

# Runtime: apenas .NET Core (sem ASP.NET)
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "FCG-Payments-WorkerService.dll"]
