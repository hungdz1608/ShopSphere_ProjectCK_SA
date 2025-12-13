# Runbook (Submission 3)

## Prerequisites
- .NET 9 SDK
- SQL Server (local or container). Update `ConnectionStrings:Default` in `src/ShopSphere.Api/appsettings*.json` and `src/ShopSphere.Worker/appsettings.json`.
- RabbitMQ reachable at the `RabbitMq` settings (default `localhost`, `guest/guest`).

## Bootstrap the database
1. From `src/ShopSphere.Api`, ensure the database exists:
   ```bash
   dotnet ef database update # if migrations are present
   # or let the API create schema automatically via EnsureCreated on first run
   ```
2. Tables needed: `Products`, `Categories`, `Orders`, `Payments`, `OutboxMessages`, `ProcessedMessages`.

## Run the API (v1)
```bash
cd src/ShopSphere.Api
ASPNETCORE_ENVIRONMENT=Development dotnet run
```
- Swagger UI: `http://localhost:5134/swagger`
- Auth: obtain token via `POST /v1/auth/login` (demo users in `appsettings.json`).
- Catalog endpoints: `/v1/products`, `/v1/categories`.
- Checkout saga entrypoint: `POST /v1/orders/checkout` with `{ "buyerEmail": "test@example.com", "totalAmount": 100 }`.

## Run the worker
```bash
cd src/ShopSphere.Worker
DOTNET_ENVIRONMENT=Development dotnet run
```
- `OutboxDispatcher` polls `OutboxMessages` and publishes to RabbitMQ.
- `OrderPaymentConsumer` listens on `order.*` routing keys, ensures idempotency using `ProcessedMessages`, and completes payments.

## Verifying the flow
1. Create a product/category as Admin or Staff.
2. Call `POST /v1/orders/checkout` → observe an `OutboxMessages` row with `order.created`.
3. Worker publishes the event; consumer marks order as `PaymentCompleted` and records `ProcessedMessages`.
4. Check `/v1/orders/{id}` or DB to confirm status and see subsequent `payment.completed` outbox entries.

## Troubleshooting
- **Duplicate events**: verify `ProcessedMessages` contains the `MessageId` (should come from RabbitMQ `MessageId` header).
- **No events leaving DB**: confirm worker connection string and RabbitMQ host; increase log level in `appsettings.json`.
- **Schema missing**: rerun API once to trigger `EnsureCreated` or add a migration for your environment.
