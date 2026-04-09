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
| **SimpleFrontend** | :5010 | Simple Blazor dashboard with data tables (default template) |
| **WebFrontend** | :5020 | AI-generated FDU-themed e-commerce course marketplace (Blazor) |

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

## V3 Plus — Blazor Frontends

### SimpleFrontend (port 5010)

A minimal Blazor Server application using the **default project template** (no custom styling). Provides a straightforward dashboard with Bootstrap tables showing raw data from every microservice through the API Gateway:

- **Dashboard** — service health status at a glance
- **Customers / Products / Orders / Payments** — auto-loaded data tables
- **Cart / Notifications** — lookup by Buyer/User ID
- **Create Order** form that triggers RabbitMQ events in real time

### WebFrontend (port 5020) — AI-Generated FDU Course Marketplace

A fully styled Blazor Server e-commerce application themed after FDU (Fairleigh Dickinson University). This frontend was built using **AI-assisted web scraping and generation**:

1. **Web scraping** — the FDU website (fdu.edu) was scraped to capture the university's visual identity: colors, typography, layout patterns, logo usage, and overall design language.
2. **AI-generated CSS/HTML** — using the scraped reference material, AI (GitHub Copilot) generated all the custom CSS, HTML structure, component layouts, and responsive design from scratch. No FDU assets, images, or copyrighted content were copied; only the visual style was used as inspiration.
3. **E-commerce course marketplace** — the concept is a course-selling platform where FDU sellers list courses/products and buyers can browse, add to cart, place orders, and receive notifications. Role-based UI adapts for Admin, Buyer, and Seller profiles.

**Key features:**
- FDU-inspired color scheme (maroon/burgundy primary, gold accents, dark tones)
- Role-based profile state (Admin / Buyer / Seller) with different navigation and dashboards
- Typed HttpClient services per microservice (no API Gateway dependency — calls services directly)
- Full CRUD pages: Customers, Products, Orders, Cart, Payments, Notifications
- Responsive layout with custom Blazor CSS isolation

> **Note:** This frontend demonstrates AI as a development accelerator. The entire UI — including CSS variables, component structure, and responsive breakpoints — was generated by AI based on the scraped design reference, then integrated with the microservices backend.

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
