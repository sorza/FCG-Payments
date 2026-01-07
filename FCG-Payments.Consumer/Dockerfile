FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["FCG-Payments.Consumer/FCG-Payments.Consumer.csproj", "FCG-Payments.Consumer/"]
COPY ["FCG-Payments.Infrastructure/FCG-Payments.Infrastructure.csproj", "FCG-Payments.Infrastructure/"]
COPY ["FCG-Payments.Application/FCG-Payments.Application.csproj", "FCG-Payments.Application/"]
COPY ["FCG-Payments.Domain/FCG-Payments.Domain.csproj", "FCG-Payments.Domain/"]

RUN dotnet restore "FCG-Payments.Consumer/FCG-Payments.Consumer.csproj"

COPY . .
WORKDIR /src/FCG-Payments.Consumer
RUN dotnet publish "FCG-Payments.Consumer.csproj" -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/runtime:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "FCG-Payments.Consumer.dll"]
