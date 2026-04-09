# V2 - Second Deliverable Changelog

**Commit:** `5a6bdbb` - *V2: RabbitMQ messaging, API Gateway (YARP), DTOs, docker-compose update*
**Date:** April 6, 2026
**Stats:** 33 files changed, +1251 lines, -341 lines

---

## What Changed

### A) Asynchronous Messaging with RabbitMQ (MassTransit)

**Objective:** Asynchronous communication between microservices using a message broker.

**New files:**

| File | Description |
|---|---|
| `OrderService/OrderService.Api/Contracts/Events.cs` | Event contracts (`OrderCreatedEvent`, `OrderCancelledEvent`, `OrderItemEvent`) |
| `NotificationService/NotificationService.Api/Contracts/Events.cs` | Same contracts replicated (shared namespace `Contracts.Events`) |
| `NotificationService/NotificationService.Api/Consumers/OrderCreatedConsumer.cs` | Consumer that receives `OrderCreatedEvent` and creates notifications for buyer and sellers |
| `NotificationService/NotificationService.Api/Consumers/OrderCancelledConsumer.cs` | Consumer that receives `OrderCancelledEvent` and notifies buyer and sellers |

**Modified files:**

| File | What changed |
|---|---|
| `OrderService/OrderService.Api/Program.cs` | Added `AddMassTransit()` with RabbitMQ configuration (host via config `RabbitMQ:Host`) |
| `OrderService/OrderService.Api/OrderService.Api.csproj` | Added `MassTransit.RabbitMQ 8.4.0` package |
| `OrderService/OrderService.Api/Controllers/OrdersController.cs` | Injected `IPublishEndpoint`; `Create` publishes `OrderCreatedEvent`; `Cancel` publishes `OrderCancelledEvent` |
| `NotificationService/NotificationService.Api/Program.cs` | Added `AddMassTransit()` with `AddConsumer<OrderCreatedConsumer>()` and `AddConsumer<OrderCancelledConsumer>()` |
| `NotificationService/NotificationService.Api/NotificationService.Api.csproj` | Added `MassTransit.RabbitMQ 8.4.0` package |

**Flow:**
1. Buyer creates an order via `POST /api/orders`
2. `OrdersController` saves the order and publishes `OrderCreatedEvent` to RabbitMQ
3. `OrderCreatedConsumer` in NotificationService receives the event and creates notifications for the buyer and each seller
4. Same flow for cancellation with `OrderCancelledEvent` -> `OrderCancelledConsumer`

---

### B) API Gateway with YARP

**Objective:** Single entry point for all microservices + data aggregation endpoint.

**New files (entire project):**

| File | Description |
|---|---|
| `ApiGateway/ApiGateway/ApiGateway.csproj` | .NET 10 project with `Yarp.ReverseProxy 2.3.0` and `Swashbuckle.AspNetCore 10.1.2` |
| `ApiGateway/ApiGateway/Program.cs` | YARP reverse proxy setup + named HttpClients for aggregation + Swagger |
| `ApiGateway/ApiGateway/appsettings.json` | YARP routes for the 6 microservices + clusters pointing to Docker internal hostnames |
| `ApiGateway/ApiGateway/Controllers/AggregateController.cs` | `GET /api/aggregate/order-details/{orderId}` - calls Order, Customer, Product in parallel |
| `ApiGateway/ApiGateway/Dockerfile` | Multi-stage build following the same pattern as other services |
| `ApiGateway/ApiGateway/appsettings.Development.json` | Logging configuration for development |

**YARP routes configured:**

| Route | Destination |
|---|---|
| `/api/customers/{**catch-all}` | `http://customerservice:8080` |
| `/api/products/{**catch-all}` | `http://productservice:8080` |
| `/api/orders/{**catch-all}` | `http://orderservice:8080` |
| `/api/notifications/{**catch-all}` | `http://notificationservice:8080` |
| `/api/cart/{**catch-all}` | `http://cartservice:8080` |
| `/api/payments/{**catch-all}` | `http://paymentservice:8080` |

**Aggregated endpoint:**
- `GET /api/aggregate/order-details/{orderId}` returns JSON with `{ order, customer, products }` combining data from 3 services in parallel.

---

### C) DTOs in All Services

**Objective:** Never expose domain entities directly in APIs; use request/response DTO records.

**New files (`Dtos/` folder in each service):**

| File | DTOs created |
|---|---|
| `CustomerService/.../Dtos/CustomerDtos.cs` | `CreateCustomerDto`, `UpdateCustomerDto`, `CustomerResponseDto`, `CustomerDetailDto`, `CustomerDashboardDto`, `PersonalDashboardDto`, `CustomerSummaryDto` |
| `OrderService/.../Dtos/OrderDtos.cs` | `CreateOrderRequestDto`, `CreateOrderItemDto`, `UpdateStatusDto`, `CancelOrderDto`, `ReturnOrderDto`, `OrderResponseDto`, `OrderItemResponseDto`, `OrderDashboardDto`, `StatusCountDto`, `BuyerDashboardDto`, `LastOrderDto`, `SellerDashboardDto` |
| `ProductService/.../Dtos/ProductDtos.cs` | `CreateProductDto`, `UpdateProductDto`, `StockUpdateDto`, `StockDecrementDto`, `DiscountUpdateDto`, `ProductResponseDto`, `ProductStockDto`, `ProductDiscountDto`, `ProductDashboardDto`, `CategoryStatsDto`, `SellerDashboardDto`, `SellerProductDto` |
| `CartService/.../Dtos/CartDtos.cs` | `AddCartItemDto`, `UpdateCartItemDto`, `CartResponseDto`, `CartItemResponseDto`, `EmptyCartDto`, `CheckoutResponseDto` |
| `PaymentService/.../Dtos/PaymentDtos.cs` | `CreatePaymentDto`, `PaymentResponseDto`, `PaymentProcessResultDto`, `PaymentDashboardDto`, `PaymentStatusStatsDto`, `PaymentMethodStatsDto` |
| `NotificationService/.../Dtos/NotificationDtos.cs` | `CreateNotificationDto`, `NotificationResponseDto`, `NotificationPageDto`, `UnreadCountDto`, `NotificationDashboardDto`, `TypeCountDto`, `UserUnreadDto`, `MarkReadResultDto` |

**Modified controllers (all 6):**

| Controller | Changes |
|---|---|
| `CustomersController.cs` | All endpoints return DTOs; `Create`/`Update` accept DTOs; added `MapToDto()` method |
| `OrdersController.cs` | Same + removed old inline classes; deleted `Models/Dtos.cs` |
| `ProductsController.cs` | Same + removed inline records `StockUpdateRequest`, `StockDecrementRequest`, `DiscountUpdateRequest` |
| `CartController.cs` | Same + removed inline request classes |
| `PaymentsController.cs` | Same + removed inline `CreatePaymentRequest` |
| `NotificationsController.cs` | Same pattern applied |

**Deleted file:**
- `OrderService/OrderService.Api/Models/Dtos.cs` - old DTOs replaced by the new `Dtos/OrderDtos.cs`

---

### D) Updated docker-compose.yml

**Services added:**

```yaml
rabbitmq:
  image: rabbitmq:3-management-alpine
  ports: 5672:5672, 15672:15672
  healthcheck: rabbitmq-diagnostics -q ping (interval 10s, timeout 5s, retries 5)

apigateway:
  build: ApiGateway/ApiGateway
  ports: 5000:8080
  depends_on: all 6 microservices
  environment: YARP cluster override variables for Docker hostnames
```

**Modified services:**

| Service | Change |
|---|---|
| `orderservice` | Added `RabbitMQ__Host: rabbitmq` + `depends_on: rabbitmq: condition: service_healthy` |
| `notificationservice` | Added `RabbitMQ__Host: rabbitmq` + `depends_on: rabbitmq: condition: service_healthy` |

---

### E) Other Changes

| File | Change |
|---|---|
| `DistributedDataAccessLab.sln` | Added `ApiGateway` project to the solution |
| `.gitignore` | Added rules for `*.db`, `*.db-shm`, `*.db-wal` |
| `CustomerService/CustomerService.Api/Dockerfile` | Minor build adjustment |
| `README.md` | Rewritten with service table, V2 section, updated tech stack |

---

## Final Architecture (V2)

```
                    +-------------------+
                    |   API Gateway     | :5000
                    |   (YARP Proxy)    |
                    +--------+----------+
                             |
     +----------+--------+---+---+--------+---------+
     |          |        |       |        |         |
 Customer  Product   Order    Cart   Payment  Notification
  :5001     :5003    :5002   :5005   :5006     :5007
                       |                         |
                       |    +--------------+     |
                       +--->|  RabbitMQ    |<----+
                            | :5672/:15672 |
                            +--------------+
                       publish           consume
                  OrderCreatedEvent  OrderCreatedConsumer
                  OrderCancelledEvent OrderCancelledConsumer
```
