# FCG-Payments

Projeto de microserviço de pagamentos em .NET 8, criado como parte de um portfólio público no GitHub. Foco em arquitetura limpa (Clean/Onion), Domain-Driven Design (DDD), integração com mensageria (Azure Service Bus) e padrões de projeto para escalabilidade, testabilidade e manutenção.

## Visão geral
- Serviço responsável por orquestrar processos de pagamento e integrar gateways (ex.: cartão de débito, PayPal).
- Comunicação assíncrona via Azure Service Bus para desacoplamento entre serviços.
- Estrutura modular em camadas (`Domain`, `Application`, `Infrastructure`, `Api`, `Consumer`).

## Tecnologias principais
- .NET 8, C#
- Entity Framework Core 8 (EF Core) + SQL Server
- Azure Service Bus (`Azure.Messaging.ServiceBus`)
- `Microsoft.Extensions.DependencyInjection` (DI)
- Projetos multi-layer (Domain, Application, Infrastructure, Api, Consumer)

## Arquitetura e padrões
- Clean Architecture / Onion Architecture
- Domain-Driven Design (entidades, agregados, serviços de domínio)
- Message-driven architecture (consumidores/handlers para tópicos/filas)
- Strategy Pattern (ex.: `DebitCardPayment`, `PaypalPayment`)
- Dependency Injection para inversão de controle e teste
- Background workers / `BackgroundService` para consumidores e jobs em segundo plano
- Separação clara de responsabilidades: casos de uso na camada `Application`, persistência e integrações na `Infrastructure`

## Estrutura do repositório
- `FCG-Payments.Domain` — Modelos e regras de negócio (entidades)
- `FCG-Payments.Application` — Casos de uso, serviços de aplicação
- `FCG-Payments.Infrastructure` — EF Core, Service Bus, estratégias de pagamento, DI (`DependencyInjection.cs`)
- `FCG-Payments.Api` — Endpoints REST
- `FCG-Payments.Consumer` — Consumidores/workers para mensageria

## Execução local
Pré-requisitos:
- .NET 8 SDK
- SQL Server ou compatível
- Namespace/connection string do Azure Service Bus (ou emulador)

Configuração:
- Ajustar `appsettings.json` ou variáveis de ambiente:
  - `ConnectionStrings:DefaultConnection` — string do banco de dados
  - `ServiceBus:ConnectionString` — connection string do Azure Service Bus

Comandos úteis:
- `dotnet build`
- `dotnet run --project FCG-Payments.Api`
- `dotnet run --project FCG-Payments.Consumer`

## SEO 
.NET 8, C#, EF Core, Azure Service Bus, Clean Architecture, Onion Architecture, DDD, Microservices, Message-driven, Strategy Pattern, Dependency Injection, BackgroundService, SQL Server, Payment Gateway, Pagamentos, Portfólio GitHub
