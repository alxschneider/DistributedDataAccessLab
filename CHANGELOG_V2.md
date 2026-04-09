# V2 — Second Deliverable Changelog

**Commit:** `5a6bdbb` — *V2: RabbitMQ messaging, API Gateway (YARP), DTOs, docker-compose update*  
**Date:** April 6, 2026  
**Stats:** 33 files changed, +1251 lines, −341 lines

---

## O que mudou

### A) Mensageria Assíncrona com RabbitMQ (MassTransit)

**Objetivo:** Comunicação assíncrona entre microsserviços usando message broker.

**Arquivos novos:**

| Arquivo | Descrição |
|---|---|
| `OrderService/OrderService.Api/Contracts/Events.cs` | Contratos dos eventos (`OrderCreatedEvent`, `OrderCancelledEvent`, `OrderItemEvent`) |
| `NotificationService/NotificationService.Api/Contracts/Events.cs` | Mesmos contratos replicados (namespace compartilhado `Contracts.Events`) |
| `NotificationService/NotificationService.Api/Consumers/OrderCreatedConsumer.cs` | Consumer que recebe `OrderCreatedEvent` e cria notificações para buyer e sellers |
| `NotificationService/NotificationService.Api/Consumers/OrderCancelledConsumer.cs` | Consumer que recebe `OrderCancelledEvent` e notifica buyer e sellers |

**Arquivos modificados:**

| Arquivo | O que mudou |
|---|---|
| `OrderService/OrderService.Api/Program.cs` | Adicionado `AddMassTransit()` com configuração RabbitMQ (host via config `RabbitMQ:Host`) |
| `OrderService/OrderService.Api/OrderService.Api.csproj` | Adicionado pacote `MassTransit.RabbitMQ 8.4.0` |
| `OrderService/OrderService.Api/Controllers/OrdersController.cs` | Injetado `IPublishEndpoint`; endpoint `Create` publica `OrderCreatedEvent`; endpoint `Cancel` publica `OrderCancelledEvent` |
| `NotificationService/NotificationService.Api/Program.cs` | Adicionado `AddMassTransit()` com `AddConsumer<OrderCreatedConsumer>()` e `AddConsumer<OrderCancelledConsumer>()` |
| `NotificationService/NotificationService.Api/NotificationService.Api.csproj` | Adicionado pacote `MassTransit.RabbitMQ 8.4.0` |

**Fluxo:**
1. Buyer cria um pedido via `POST /api/orders`
2. `OrdersController` salva o pedido e publica `OrderCreatedEvent` no RabbitMQ
3. `OrderCreatedConsumer` no NotificationService recebe o evento e cria notificações para o buyer e cada seller
4. Mesmo fluxo para cancelamento com `OrderCancelledEvent` → `OrderCancelledConsumer`

---

### B) API Gateway com YARP

**Objetivo:** Ponto de entrada único para todos os microsserviços + endpoint de agregação.

**Arquivos novos (projeto inteiro):**

| Arquivo | Descrição |
|---|---|
| `ApiGateway/ApiGateway/ApiGateway.csproj` | Projeto .NET 10 com pacotes `Yarp.ReverseProxy 2.3.0` e `Swashbuckle.AspNetCore 10.1.2` |
| `ApiGateway/ApiGateway/Program.cs` | Configuração do YARP reverse proxy + HttpClients nomeados para agregação + Swagger |
| `ApiGateway/ApiGateway/appsettings.json` | Rotas YARP para os 6 microsserviços + clusters apontando para hostnames internos do Docker |
| `ApiGateway/ApiGateway/Controllers/AggregateController.cs` | Endpoint `GET /api/aggregate/order-details/{orderId}` — chama Order, Customer e Product em paralelo e retorna resposta combinada |
| `ApiGateway/ApiGateway/Dockerfile` | Build multi-stage seguindo o padrão dos outros serviços |
| `ApiGateway/ApiGateway/appsettings.Development.json` | Config de logging para desenvolvimento |

**Rotas configuradas no YARP:**

| Rota | Destino |
|---|---|
| `/api/customers/{**catch-all}` | `http://customerservice:8080` |
| `/api/products/{**catch-all}` | `http://productservice:8080` |
| `/api/orders/{**catch-all}` | `http://orderservice:8080` |
| `/api/notifications/{**catch-all}` | `http://notificationservice:8080` |
| `/api/cart/{**catch-all}` | `http://cartservice:8080` |
| `/api/payments/{**catch-all}` | `http://paymentservice:8080` |

**Endpoint agregado:**
- `GET /api/aggregate/order-details/{orderId}` retorna JSON com `{ order, customer, products }` combinando dados de 3 serviços em paralelo.

---

### C) DTOs em Todos os Serviços

**Objetivo:** Não expor entidades de domínio diretamente nas APIs; usar records de request/response.

**Arquivos novos (pasta `Dtos/` em cada serviço):**

| Arquivo | DTOs criados |
|---|---|
| `CustomerService/CustomerService.Api/Dtos/CustomerDtos.cs` | `CreateCustomerDto`, `UpdateCustomerDto`, `CustomerResponseDto`, `CustomerDetailDto`, `CustomerDashboardDto`, `PersonalDashboardDto`, `CustomerSummaryDto` |
| `OrderService/OrderService.Api/Dtos/OrderDtos.cs` | `CreateOrderRequestDto`, `CreateOrderItemDto`, `UpdateStatusDto`, `CancelOrderDto`, `ReturnOrderDto`, `OrderResponseDto`, `OrderItemResponseDto`, `OrderDashboardDto`, `StatusCountDto`, `BuyerDashboardDto`, `LastOrderDto`, `SellerDashboardDto` |
| `ProductService/ProductService.Api/Dtos/ProductDtos.cs` | `CreateProductDto`, `UpdateProductDto`, `StockUpdateDto`, `StockDecrementDto`, `DiscountUpdateDto`, `ProductResponseDto`, `ProductStockDto`, `ProductDiscountDto`, `ProductDashboardDto`, `CategoryStatsDto`, `SellerDashboardDto`, `SellerProductDto` |
| `CartService/CartService.Api/Dtos/CartDtos.cs` | `AddCartItemDto`, `UpdateCartItemDto`, `CartResponseDto`, `CartItemResponseDto`, `EmptyCartDto`, `CheckoutResponseDto` |
| `PaymentService/PaymentService.Api/Dtos/PaymentDtos.cs` | `CreatePaymentDto`, `PaymentResponseDto`, `PaymentProcessResultDto`, `PaymentDashboardDto`, `PaymentStatusStatsDto`, `PaymentMethodStatsDto` |
| `NotificationService/NotificationService.Api/Dtos/NotificationDtos.cs` | `CreateNotificationDto`, `NotificationResponseDto`, `NotificationPageDto`, `UnreadCountDto`, `NotificationDashboardDto`, `TypeCountDto`, `UserUnreadDto`, `MarkReadResultDto` |

**Controllers modificados (todos os 6):**

| Controller | Mudanças |
|---|---|
| `CustomersController.cs` | Todos endpoints retornam DTOs; `Create`/`Update` aceitam DTOs; adicionado método `MapToDto()` |
| `OrdersController.cs` | Idem + removidas classes inline antigas; deletado `Models/Dtos.cs` antigo |
| `ProductsController.cs` | Idem + removidas records inline `StockUpdateRequest`, `StockDecrementRequest`, `DiscountUpdateRequest` |
| `CartController.cs` | Idem + removidas classes inline de request |
| `PaymentsController.cs` | Idem + removida `CreatePaymentRequest` inline |
| `NotificationsController.cs` | Idem |

**Arquivo deletado:**
- `OrderService/OrderService.Api/Models/Dtos.cs` — DTOs antigos substituídos pelo novo `Dtos/OrderDtos.cs`

---

### D) docker-compose.yml Atualizado

**Serviços adicionados:**

```yaml
rabbitmq:
  image: rabbitmq:3-management-alpine
  ports: 5672:5672, 15672:15672
  healthcheck: rabbitmq-diagnostics -q ping (interval 10s, timeout 5s, retries 5)

apigateway:
  build: ApiGateway/ApiGateway
  ports: 5000:8080
  depends_on: todos os 6 microsserviços
  environment: variáveis de override dos clusters YARP para hostnames Docker
```

**Serviços modificados:**

| Serviço | Mudança |
|---|---|
| `orderservice` | Adicionado `RabbitMQ__Host: rabbitmq` + `depends_on: rabbitmq: condition: service_healthy` |
| `notificationservice` | Adicionado `RabbitMQ__Host: rabbitmq` + `depends_on: rabbitmq: condition: service_healthy` |

---

### E) Outros

| Arquivo | Mudança |
|---|---|
| `DistributedDataAccessLab.sln` | Adicionado projeto `ApiGateway` na solution |
| `.gitignore` | Adicionadas regras para `*.db`, `*.db-shm`, `*.db-wal` |
| `CustomerService/CustomerService.Api/Dockerfile` | Ajuste menor no build |
| `README.md` | Reescrito com tabela de serviços, seção V2, tech stack atualizado |

---

## Arquitetura Final (V2)

```
                    ┌─────────────────┐
                    │   API Gateway   │ :5000
                    │   (YARP Proxy)  │
                    └────────┬────────┘
                             │
         ┌───────────┬───────┼───────┬────────────┬──────────┐
         │           │       │       │            │          │
    ┌────┴────┐ ┌────┴───┐ ┌─┴──┐ ┌──┴───┐ ┌─────┴──┐ ┌────┴────┐
    │Customer │ │Product │ │Order│ │Cart  │ │Payment │ │Notific. │
    │ :5001   │ │ :5003  │ │:5002│ │:5005 │ │ :5006  │ │ :5007   │
    └─────────┘ └────────┘ └──┬──┘ └──────┘ └────────┘ └────┬────┘
                              │                              │
                              │    ┌──────────────┐          │
                              └────► RabbitMQ     ◄──────────┘
                                   │ :5672/:15672 │
                                   └──────────────┘
                              publish               consume
                        OrderCreatedEvent      OrderCreatedConsumer
                        OrderCancelledEvent    OrderCancelledConsumer
```
