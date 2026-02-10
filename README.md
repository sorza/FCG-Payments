# 💳 FCG-Payments

Microsserviço de Pagamentos — Processamento de transações com Strategy Pattern e Event Sourcing.

[![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![Clean Architecture](https://img.shields.io/badge/Architecture-Clean-green)](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
[![Strategy Pattern](https://img.shields.io/badge/Pattern-Strategy-purple)](https://refactoring.guru/design-patterns/strategy)
[![Event Sourcing](https://img.shields.io/badge/Pattern-Event%20Sourcing-red)](https://martinfowler.com/eaaDev/EventSourcing.html)

## 📝 Descrição

**FCG-Payments** processa transações financeiras:

- ✅ **Múltiplos métodos**: Cartão Débito, Crédito, PayPal, PIX
- ✅ **Strategy Pattern**: Algoritmos intercambiáveis por tipo de pagamento
- ✅ **Event Sourcing**: Auditoria completa de todas as transações
- ✅ **Integração**: Publica PaymentProcessed → Libraries adiciona game
- ✅ **Autorização**: Apenas usuários autenticados

---

## 🚀 Pré-requisitos

- .NET 8 SDK
- SQL Server
- MongoDB (Event Store)
- Azure Service Bus
- Docker (opcional)

---

## ⚙️ Configuração Local

**appsettings.json**:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=PaymentsDb;Trusted_Connection=True;"
  },
  "ServiceBus": {
    "ConnectionString": "<connection-string>",
    "Topics": { "Payments": "payments-events" }
  },
  "MongoSettings": {
    "ConnectionString": "mongodb://localhost:27017",
    "Database": "EventStoreDb"
  },
  "Jwt": {
    "Key": "9y4XJg0aTphzFJw3TvksRvqHXd+Q4VB8f7ZvU08N+9Q=",
    "Issuer": "FGC-Users"
  },
  "Services": {
    "LibrariesApi": "https://localhost:7004"
  }
}
```

---

## 🚀 Como Executar

### Migrations
```bash
cd FCG-Payments.Api
dotnet ef database update
```

### API
```bash
cd FCG-Payments.Api
dotnet run
# https://localhost:7003/swagger
```

### Consumer
```bash
cd FCG-Payments.Consumer
dotnet run
```

---

## 📊 Endpoints

| Método | Endpoint           | Autenticação | Descrição |
|--------|-------------------|--------------|-----------|
| GET    | `/api`            | Qualquer     | Listar pagamentos |
| GET    | `/api/{id}`       | Qualquer     | Obter pagamento |
| POST   | `/api`            | Qualquer     | Criar pagamento |
| POST   | `/api/{id}/process`| Qualquer     | Processar pagamento |
| DELETE | `/api/{id}`       | Qualquer     | Cancelar pagamento |

---

## 🧪 Testar

```bash
# Criar pagamento
curl -X POST https://localhost:7003/api \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  -d '{
    "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "gameId": "7b8c9d0e-1f2a-3b4c-5d6e-7f8a9b0c1d2e",
    "amount": 59.99,
    "paymentType": "DebitCard"
  }'

# Processar pagamento
curl -X POST https://localhost:7003/api/{paymentId}/process \
  -H "Authorization: Bearer <token>"
```

---

## 🐳 Docker

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["FCG-Payments.Api/", "."]
RUN dotnet restore && dotnet publish -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .
EXPOSE 8080
ENTRYPOINT ["dotnet", "FCG-Payments.Api.dll"]
```

---

## ☸️ Kubernetes

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: fcg-payments
spec:
  replicas: 2
  selector:
    matchLabels:
      app: fcg-payments
  template:
    metadata:
      labels:
        app: fcg-payments
    spec:
      containers:
      - name: fcg-payments
        image: fcg-payments:latest
        ports:
        - containerPort: 8080
```

---

## 📚 Referências

- [Strategy Pattern](https://refactoring.guru/design-patterns/strategy)
- [Event Sourcing](https://martinfowler.com/eaaDev/EventSourcing.html)
- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)

---

**FIAP Tech Challenge — Fase 4**
