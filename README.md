# DistributedDataAccessLab

**E-Commerce Marketplace Backend — FDU MSACS CSCI_6844_V1 Programming for the Internet**

Fully containerized e-commerce backend built with .NET 10 and Docker Compose. Six independent microservices, each with its own SQLite database, communicating through synchronous HTTP REST calls and asynchronous messaging via RabbitMQ.

---

## Services

| Service | Port | Description |
|---|---|---|
| **API Gateway** | :5000 | YARP reverse proxy routing + aggregated endpoint |
| **CustomerService** | :5001 | Manage buyers and sellers, role-based visibility, personal dashboards |
| **OrderService** | :5002 | Create and track orders, status transitions, cancel/return flows with stock restoration |
| **ProductService** | :5003 | Product catalog, stock management, discount system, seller-specific endpoints |
| **CartService** | :5005 | Shopping cart with add/update/remove items, checkout orchestration |
| **PaymentService** | :5006 | Payment creation, processing (mock with 90% approval rate), refunds |
| **NotificationService** | :5007 | Store and retrieve notifications, mark as read, RabbitMQ consumers |
| **RabbitMQ** | :5672 / :15672 | Message broker (management UI on 15672) |

## How to Run

```bash
docker compose up --build
```

All services start with a single command. Each service auto-migrates its database on startup.  
API Gateway is available at `http://localhost:5000`.

---

## V2 — Second Deliverable Changes

### A) Asynchronous Messaging with RabbitMQ (MassTransit)

- Added **MassTransit.RabbitMQ 8.4.0** to `OrderService` (publisher) and `NotificationService` (consumers).
- **OrderCreatedEvent** — published when a new order is created; consumed by `OrderCreatedConsumer` in NotificationService, which creates notifications for the buyer and each seller.
- **OrderCancelledEvent** — published when an order is cancelled; consumed by `OrderCancelledConsumer` in NotificationService, which notifies buyer and sellers.
- Event contracts defined in `Contracts/Events.cs` in both services (shared namespace `Contracts.Events`).
- RabbitMQ declared as a service in `docker-compose.yml` with health checks; OrderService and NotificationService depend on it.

### B) API Gateway with YARP

- New **ApiGateway** project using `Yarp.ReverseProxy 2.3.0`.
- Reverse proxy routes configured in `appsettings.json` for all six microservices (`/api/customers`, `/api/products`, `/api/orders`, `/api/notifications`, `/api/cart`, `/api/payments`).
- **Aggregated endpoint**: `GET /api/aggregate/order-details/{orderId}` — calls OrderService, CustomerService, and ProductService in parallel and returns a combined response with order, customer, and product data.

### C) DTOs in All Services

- Created `Dtos/` folder in each service with request and response DTO records.
- All controllers updated to accept DTOs on input and return DTOs on output instead of exposing domain entities or anonymous types.
- Services affected: CustomerService, OrderService, ProductService, CartService, PaymentService, NotificationService.

### D) Updated docker-compose.yml

- Added `rabbitmq` service (rabbitmq:3-management-alpine, ports 5672/15672, health check).
- Added `apigateway` service (port 5000, depends on all microservices).
- Added `RabbitMQ__Host` environment variable to OrderService and NotificationService.
- Added `depends_on: rabbitmq: condition: service_healthy` to OrderService and NotificationService.

---

## Key Design Decisions

- **Database-per-service** — each microservice owns its own SQLite database, no shared data stores
- **Synchronous communication** via typed HttpClients with interface abstractions (DIP)
- **Asynchronous communication** via RabbitMQ + MassTransit for event-driven notifications
- **API Gateway** centralizes routing and provides data aggregation across services
- **Service discovery** through Docker Compose DNS, URLs injected via environment variables
- **Soft deletes** instead of hard deletes to preserve data integrity
- **Business rules in controllers** for simplicity — in production, would extract to a service/use-case layer
- **Auto-migration** on startup for easy testing and deployment
- **Price and product name snapshotted** on OrderItem to preserve historical accuracy

## Tech Stack

- .NET 10 / ASP.NET Core
- Entity Framework Core + SQLite
- Docker + Docker Compose
- MassTransit + RabbitMQ (async messaging)
- YARP (reverse proxy / API Gateway)
- Swagger/OpenAPI for each service

## Note

`appsettings` files are not in `.gitignore` on purpose — this is an educational project.

```bash
docker compose up --build
```
