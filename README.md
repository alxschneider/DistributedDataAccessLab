DistributedDataAccessLab
E-Commerce Marketplace Backend — FDU MSACS CSCI_6844_V1 Programming for the Internet
Fully containerized e-commerce backend built with .NET 10 and Docker Compose. Six independent microservices, each with its own SQLite database, communicating through synchronous HTTP REST calls.
Services

CustomerService (:5001) — Manage buyers and sellers, role-based visibility, personal dashboards
OrderService (:5002) — Create and track orders, status transitions, cancel/return flows with stock restoration
ProductService (:5003) — Product catalog, stock management, discount system, seller-specific endpoints
NotificationService (:5004) — Store and retrieve notifications for buyers and sellers, mark as read
CartService (:5005) — Shopping cart with add/update/remove items, checkout orchestration
PaymentService (:5006) — Payment creation, processing (mock with 90% approval rate), refunds

How to Run
docker compose up --build
All six services start with a single command. Each service auto-migrates its database on startup.
Key Design Decisions

Database-per-service — each microservice owns its own SQLite database, no shared data stores
Inter-service communication via typed HttpClients with interface abstractions (DIP)
Service discovery through Docker Compose DNS, URLs injected via environment variables
Soft deletes instead of hard deletes to preserve data integrity
Business rules in controllers for simplicity — in production, would extract to a service/use-case layer
Auto-migration on startup for easy testing and deployment
Price and product name snapshotted on OrderItem to preserve historical accuracy

Tech Stack

.NET 10 / ASP.NET Core
Entity Framework Core + SQLite
Docker + Docker Compose
Swagger/OpenAPI for each service

Note
appsettings files are not in .gitignore on purpose — this is an educational projec, same for db seeds.

Troubleshoot:
target productservice: failed to solve: mcr.microsoft.com/dotnet/aspnet:10.0-alpine: failed to resolve source metadata for mcr.microsoft.com/dotnet/aspnet:10.0-alpine: failed to do request: Head "https://mcr.microsoft.com/v2/dotnet/aspnet/manifests/10.0-alpine": EOF

getting these msg sometimes when trying to run composer, for me, at least, is a connection issue, just retry and it will work!
