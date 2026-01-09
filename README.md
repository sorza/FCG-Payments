# 💳 FCG-Payments - Payment Processing Service

> **Microsserviço de Pagamentos** - Processamento de transações com Strategy Pattern e Event Sourcing

[![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![Clean Architecture](https://img.shields.io/badge/Architecture-Clean-green)](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
[![Strategy Pattern](https://img.shields.io/badge/Pattern-Strategy-purple)](https://refactoring.guru/design-patterns/strategy)
[![Event Sourcing](https://img.shields.io/badge/Pattern-Event%20Sourcing-red)](https://martinfowler.com/eaaDev/EventSourcing.html)
[![DDD](https://img.shields.io/badge/Design-Domain--Driven-blue)](https://martinfowler.com/bliki/DomainDrivenDesign.html)

## 🎯 O que é este projeto?

**FCG-Payments** é o **microsserviço de processamento de pagamentos** responsável por gerenciar transações financeiras usando **Strategy Pattern** para diferentes métodos de pagamento, e manter **auditoria completa** através de Event Sourcing. Demonstra aplicação de padrões de design e gestão de transações distribuídas.

### Responsabilidades Principais
- ✅ Processamento de pagamentos com múltiplos métodos (Cartão de Débito, Cartão de Crédito, PayPal, PIX)
- ✅ **Strategy Pattern** para algoritmos de pagamento intercambiáveis (simulados para fins de demonstração)
- ✅ Event Sourcing: histórico imutável de todas as transações
- ✅ Publicação de eventos de domínio (PaymentCreated, PaymentProcessed)
- ✅ Consumo de eventos de Users/Games para validação de pagamentos
- ✅ Autorização: apenas usuários autenticados podem processar pagamentos
- ✅ Integração com Libraries Service para adicionar jogos após pagamento aprovado

---

## 🚀 Tecnologias e Padrões Aplicados

### Stack Técnico Completo
| Tecnologia | Propósito | Conceito Aplicado |
|------------|-----------|-------------------|
| **ASP.NET Core 8** | Web API Framework | RESTful API, Dependency Injection |
| **Entity Framework Core 8** | ORM | Code-First, Migrations, DbContext |
| **SQL Server** | Banco Relacional | Persistência de transações |
| **MongoDB (Cosmos DB)** | NoSQL Document Store | Event Store (audit trail) |
| **Azure Service Bus** | Message Broker | Topic-based routing, Event distribution |
| **Strategy Pattern** | Behavioral Pattern | Algoritmos de pagamento intercambiáveis |
| **FluentValidation** | Validação | Regras de negócio declarativas |
| **Swagger/Swashbuckle** | API Documentation | OpenAPI 3.0, Interactive testing |
| **Docker** | Containerização | Portabilidade e deployment |
| **HttpClient** | HTTP Communication | Chamadas para Libraries API |

### Padrões de Design Implementados

#### 🎯 **Strategy Pattern (Gang of Four)**
```csharp
// Abstração
public interface IPaymentStrategy
{
    Task<bool> Pay(Payment payment);
}

// Estratégias concretas (simuladas)
public class DebitCardPayment : IPaymentStrategy 
{
    public async Task<bool> Pay(Payment payment)
    {
        // Simula processamento de cartão de débito
        Console.WriteLine($"Processando pagamento por {payment.PaymentType}...");
        return true; // Aprovado
    }
}

public class PaypalPayment : IPaymentStrategy 
{
    public async Task<bool> Pay(Payment payment)
    {
        // Simula falha de PayPal
        Console.WriteLine($"Processando pagamento por {payment.PaymentType}...");
        return false; // Rejeitado
    }
}

public class PixPayment : IPaymentStrategy { }
public class CreditCardPayment : IPaymentStrategy { }

// Factory para selecionar estratégia
public class PaymentFactory
{
    private readonly Dictionary<EPaymentType, IPaymentStrategy> _strategies;
    
    public IPaymentStrategy Resolve(EPaymentType type)
    {
        return _strategies[type];
    }
}
```

**Vantagens**:
- ✅ Adicionar novos métodos de pagamento sem modificar código existente (Open/Closed Principle)
- ✅ Cada método tem sua lógica isolada e testável
- ✅ Seleção dinâmica de estratégia em runtime
- ✅ Facilita testes unitários (mock de cada estratégia)

#### 🏗️ **Clean Architecture (Onion Architecture)**
```
┌─────────────────────────────────────┐
│   API Layer (PaymentController)     │  ← Apresentação
├─────────────────────────────────────┤
│ Application Layer (PaymentService)  │  ← Casos de Uso
├─────────────────────────────────────┤
│  Domain Layer (Payment Entity)      │  ← Lógica de Negócio
├─────────────────────────────────────┤
│Infrastructure (EF, Strategies, SB)  │  ← Detalhes Técnicos
└─────────────────────────────────────┘
```

#### 📊 **Domain-Driven Design (DDD)**
- **Aggregates**: Payment como aggregate root
- **Value Objects**: Amount (validação de valor monetário), PaymentMethod
- **Domain Events**: PaymentProcessedEvent, PaymentFailedEvent
- **Repositories**: Abstração de persistência
- **Services**: Serviços de domínio para lógica transacional

#### 🔄 **Event Sourcing**
- **Event Store**: MongoDB armazena TODOS os eventos de pagamento
- **Immutable Events**: Histórico completo de criação, processamento, falhas
- **Compliance**: Auditoria para regulamentações financeiras (PCI-DSS, LGPD)
- **Replay**: Reconstruir estado de transações em qualquer momento

#### 📨 **Event-Driven Architecture (EDA)**
- **Domain Events**: Fatos financeiros ocorridos (PaymentProcessed)
- **Integration Events**: Comunicação com outros bounded contexts
- **Eventual Consistency**: Confirmação assíncrona com outros serviços
- **Dead Letter Queue**: Retry automático para falhas temporárias

---

## 📐 Estrutura do Projeto

```
FCG-Payments/
├── FCG-Payments.Api/              # Controllers, Middleware
│   ├── Controllers/
│   │   └── PaymentController.cs   # Endpoints REST
│   └── Program.cs                 # DI Container, JWT Config
│
├── FCG-Payments.Application/      # Casos de Uso, DTOs
│   ├── DTOs/
│   │   ├── CreatePaymentRequest.dto.cs
│   │   └── ProcessPaymentRequest.dto.cs
│   ├── Services/
│   │   └── PaymentService.cs      # Orquestração de pagamentos
│   └── Validators/
│       └── CreatePaymentValidator.cs
│
├── FCG-Payments.Domain/           # Entidades, Enums, Interfaces
│   ├── Entities/
│   │   ├── CreditCardPayment.cs
│   │   ├── PaypalPayment.cs
│   │   ├── PixPayment.cs
│   │   └── PaymentFactory.cs
│   │   ├── EPaymentStatus.cs      # Pending, Completed, Failed
│   │   └── EPaymentType.cs        # DebitCard, PayPal, PIX
│   ├── Events/
│   │   ├── PaymentCreatedEvent.cs
│   │   └── PaymentProcessedEvent.cs
│   └── Interfaces/
│       ├── IPaymentRepository.cs
│       └── IPaymentStrategy.cs
│
├── FCG-Payments.Infrastructure/   # EF Core, Strategies, Service Bus
│   ├── Data/
│   │   ├── AppDbContext.cs
│   │   └── Migrations/
│   ├── Repositories/
│   │   └── PaymentRepository.cs
│   ├── Strategies/
│   │   ├── DebitCardPayment.cs
│   │   ├── PaypalPayment.cs
│   │   └── PixPayment.cs
│   ├── EventStore/
│   │   └── MongoEventStore.cs
│   └── Messaging/
│       └── ServiceBusPublisher.cs
│
└── FCG-Payments.Consumer/         # Background Service
    └── Workers/
        ├── PaymentsEventsConsumer.cs
        └── LibrariesEventsConsumer.cs
```

---

## ⚙️ Configuração e Execução

### Pré-requisitos
- .NET 8 SDK
- SQL Server (local ou Azure)
- MongoDB (local, Docker ou Cosmos DB)
- Azure Service Bus namespace
- JWT Key (compartilhada com FCG-Users)

### Configuração (appsettings.json)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=PaymentsDb;Trusted_Connection=True;"
  },
  "ServiceBus": {
    "ConnectionString": "<service-bus-connection-string>",
    "Topics": {
      "Payments": "payments-events"
    },
    "Subscriptions": {
      "Payments": "payments-subscription"
    }
  },
  "MongoSettings": {
    "ConnectionString": "mongodb://localhost:27017",
    "Database": "EventStoreDb",
    "Collection": "Events"
  },
  "Jwt": {
    "Key": "9y4XJg0aTphzFJw3TvksRvqHXd+Q4VB8f7ZvU08N+9Q=",
    "Issuer": "FGC-Users",
    "Audience": "API"
  },
  "Services": {
    "LibrariesApi": "https://localhost:7004"
  }
}
```

### Executar Migrations
```powershell
cd FCG-Payments.Api
dotnet ef database update
```

### Executar API
```powershell
cd FCG-Payments.Api
dotnet run
# API disponível em: https://localhost:7003
```

### Executar Consumer
```powershell
cd FCG-Payments.Consumer
dotnet run
```

---

## 🔐 Endpoints e Autorização

### Matriz de Autorização

| Método | Endpoint | Autorização | Descrição |
|--------|----------|-------------|-----------|
| GET | `/api` | [Authorize] | Listar pagamentos do usuário autenticado |
| GET | `/api/{id}` | [Authorize] | Obter pagamento por ID (apenas do próprio usuário) |
| POST | `/api` | [Authorize] | Criar novo pagamento |
| POST | `/api/{id}/process` | [Authorize] | Processar pagamento pendente |
| DELETE | `/api/{id}` | [Authorize] | Cancelar pagamento (apenas se Pending) |

### Exemplo de Request (Processar Pagamento)

**Obter token JWT**:
```bash
curl -X POST https://localhost:7001/api/auth \
  -H "Content-Type: application/json" \
  -d '{"email": "user@fcg.com", "password": "Senha@123"}'
```

**Criar pagamento**:
```bash
curl -X POST https://localhost:7003/api \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  -d '{
    "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "gameId": "7b8c9d0e-1f2a-3b4c-5d6e-7f8a9b0c1d2e",
    "amount": 59.99,
    "paymentType": "DebitCard"
  }'
```

**Response**:
```json
{
  "paymentId": "a1b2c3d4-e5f6-7g8h-9i0j-k1l2m3n4o5p6",
  "status": "Pending",
  "amount": 59.99,
  "paymentType": "DebitCard",
  "createdAt": "2026-01-09T10:00:00Z"
}
```

**Processar pagamento**:
```bash
curl -X POST https://localhost:7003/api/{paymentId}/process \
  -H "Authorization: Bearer <token>"
```

**Response (Sucesso)**:
```json
{
  "paymentId": "a1b2c3d4-e5f6-7g8h-9i0j-k1l2m3n4o5p6",
  "status": "Completed",
  "processedAt": "2026-01-09T10:00:30Z",
  "transactionId": "TXN-123456789"
}
```

---

## 🏛️ Arquitetura - Diagrama Mermaid

```mermaid
graph TB
    subgraph "External Access"
        Client[Authenticated User<br/>JWT Token]
        APIM[Azure API Management]
    end
    
    subgraph "FCG-Payments Microservice"
        API[Payments API<br/>PaymentController]
        AppService[Payment Service<br/>Application Layer]
        
        subgraph "Strategy Pattern"
            StrategyFactory[Payment Strategy Factory]
            DebitCard[Debit Card Strategy]
            PayPal[PayPal Strategy]
            PIX[PIX Strategy]
        end
        
        subgraph "Data Persistence"
            SQL[(SQL Server<br/>Payments Table)]
            Mongo[(MongoDB<br/>Event Store)]
        end
        
        subgraph "Messaging"
            SB[Azure Service Bus<br/>payments-events]
        end
        
        Consumer[Payments Consumer<br/>Background Service]
    end
    
    subgraph "Dependent Microservices"
        Libraries[Libraries Service<br/>Add game to library]
    end
    
    Client -->|POST /api<br/>Auth: Bearer| APIM
    APIM -->|Validate JWT| API
    API -->|Create Payment| AppService
    AppService -->|Select Strategy| StrategyFactory
    StrategyFactory -->|DebitCard?| DebitCard
    StrategyFactory -->|PayPal?| PayPal
    StrategyFactory -->|PIX?| PIX
    
    DebitCard -->|Simulate Processing| DebitCard
    PayPal -->|Simulate Processing| PayPal
    
    AppService -->|Save Payment| SQL
    AppService -->|Append Event| Mongo
    AppService -->|Publish Event| SB
    
    SB -->|PaymentProcessedEvent| Libraries
    SB -->|Payment Events| Consumer
    
    Libraries -->|Add Game to Library| Libraries
    
    style DebitCard fill:#4CAF50
    style PayPal fill:#0070BA
    style PIX fill:#00C9A7
```

---

## 🔄 Fluxo de Processamento - Sequence Diagram

```mermaid
sequenceDiagram
    participant User
    participant PaymentsAPI
    participant PaymentService
    participant StrategyFactory
    participant DebitCardStrategy
    participant SQL
    participant EventStore
    participant ServiceBus
    participant LibrariesAPI
    
    User->>PaymentsAPI: POST /api/{id}/process
    PaymentsAPI->>PaymentsAPI: Validate JWT
    PaymentsAPI->>PaymentService: ProcessPaymentAsync(id)
    PaymentService->>StrategyFactory: Resolve(paymentType)
    StrategyFactory-->>PaymentService: DebitCardStrategy
    PaymentService->>DebitCardStrategy: Pay(payment)
    DebitCardStrategy->>DebitCardStrategy: Simulate Processing
    DebitCardStrategy-->>PaymentService: true/false (approved/rejected)
    PaymentService->>SQL: Save payment status
    PaymentService->>EventStore: Append PaymentProcessedEvent
    PaymentService->>ServiceBus: Publish PaymentProcessedEvent
    ServiceBus->>LibrariesAPI: PaymentProcessedEvent
    LibrariesAPI->>LibrariesAPI: Add game to user library
    LibrariesAPI-->>ServiceBus: Acknowledged
```

---

## 🧪 Padrões de Código Demonstrados

### Strategy Pattern Implementation
```csharp
// Factory para selecionar estratégia
public class PaymentFactory : IPaymentResolver
{
    private readonly IServiceProvider _serviceProvider;
    
    public PaymentFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    
    public IPaymentStrategy GetStrategy(EPaymentType type)
    {
        return type switch
        {
            EPaymentType.DebitCard => _serviceProvider.GetRequiredService<DebitCardPayment>(),
            EPaymentType.PayPal => _serviceProvider.GetRequiredService<PaypalPayment>(),
            EPaymentType.PIX => _serviceProvider.GetRequiredService<PixPayment>(),
            _ => throw new NotSupportedException($"Payment type {type} not supported")
        };
    }
}

// Uso no serviço
public async Task<Payment> ProcessPaymentAsync(Guid paymentId)
{
    var payment = await _paymentRepository.GetByIdAsync(paymentId);
    
    // Seleciona estratégia baseada no tipo de pagamento
    var strategy = _paymentFactory.GetStrategy(payment.PaymentType);
    
    // Executa o processamento (simulado)
    var success = await strategy.Pay(payment);
    
    if (success)
    {
        payment.Status = EPaymentStatus.Completed;
        
        // Publica evento
        await _eventPublisher.PublishAsync(new PaymentProcessedEvent
        {
            PaymentId = payment.Id,
            UserId = payment.UserId,
            GameId = payment.GameId,
            Amount = payment.Amount,
            ProcessedAt = DateTime.UtcNow
        });
        
        // Chama Libraries API para adicionar jogo à biblioteca
        var librariesClient = _httpClientFactory.CreateClient("LibrariesApi");
        await librariesClient.PostAsJsonAsync("/api", new { 
            UserId = payment.UserId, 
            GameId = payment.GameId 
        });
    }
    else
    {
        payment.Status = EPaymentStatus.Failed;
    }
    
    await _paymentRepository.UpdateAsync(payment);
    return payment;
}
```

### **Idempotency (Previne Processamento Duplicado)**
Previne o processamento duplicado do mesmo pagamento:
```csharp
public async Task<Payment> ProcessPaymentAsync(Guid paymentId)
{
    var payment = await _paymentRepository.GetByIdAsync(paymentId);
    
    // Verifica se já foi processado
    if (payment.Status == EPaymentStatus.Completed)
        return payment; // Já processado, retorna sem fazer nada
    
    // Processa apenas se estiver pendente
    if (payment.Status == EPaymentStatus.Pending)
    {
        var strategy = _paymentFactory.GetStrategy(payment.PaymentType);
        var success = await strategy.Pay(payment);
        
        payment.Status = success ? EPaymentStatus.Completed : EPaymentStatus.Failed;
        await _paymentRepository.UpdateAsync(payment);
    }
    
    return payment;
}
```

### **Event-Driven Integration**
O pagamento aprovado publica um evento (`PaymentProcessedEvent`) no Azure Service Bus que é consumido pelo Libraries Service para adicionar o jogo à biblioteca do usuário. Demonstra comunicação assíncrona entre microserviços.

---

## 📚 Referências Técnicas

- [Strategy Pattern (Refactoring Guru)](https://refactoring.guru/design-patterns/strategy)
- [Event Sourcing (Martin Fowler)](https://martinfowler.com/eaaDev/EventSourcing.html)
- [Clean Architecture (Uncle Bob)](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [Microservices Patterns (Chris Richardson)](https://microservices.io/patterns/data/event-sourcing.html)
- [Saga Pattern](https://microservices.io/patterns/data/saga.html)
