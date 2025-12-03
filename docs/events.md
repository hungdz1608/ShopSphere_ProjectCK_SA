# Messaging Plan (Submission 2)

## Transport
- **Broker**: RabbitMQ (topic exchange `shopsphere.events`).
- **Publisher**: `RabbitMqEventPublisher` produces JSON messages.
- **Connection**: configured via `RabbitMq` section (host, username, password, exchange).

## Event contracts
- **product.created**
  - Payload: `{ id, name, price, stock, categoryId }`
- **product.updated**
  - Payload: `{ id, name, price, stock, categoryId, updatedAt }`
- **product.deleted**
  - Payload: `{ id, name, categoryId }`

Each message is wrapped with `eventName` and `occurredAt` metadata before publishing.

## Publishing points
- Emitted from `ProductService` after successful create/update/delete operations.

## Subscription notes
- Exchange type `topic` allows downstream services to bind by routing keys (e.g., `product.*`).
- Consumers should implement retry/backoff and idempotency (planned for Submission 3) when handling messages.
