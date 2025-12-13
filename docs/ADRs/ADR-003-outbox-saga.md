# ADR-003: Outbox + Saga for Reliable Checkout

## Context
Submission 3 requires reliable event delivery, idempotent consumers, and a simple saga for checkout. Directly publishing to RabbitMQ from the API risked lost messages and duplicated side effects when retries occurred.

## Decision
- Use an **Outbox pattern** in the API: `OutboxEventPublisher` writes events to `OutboxMessages` within the same transaction scope as domain changes.
- Run a **worker** with two background services:
  - `OutboxDispatcher` polls pending rows and publishes to RabbitMQ with `MessageId` headers.
  - `OrderPaymentConsumer` subscribes to `order.*` events, checks `ProcessedMessages` for idempotency, and progresses the payment step.
- Model a **checkout saga**: order + payment are created, `order.created` triggers payment completion, and emits `payment.completed` for downstream listeners.
- Expose **/v1** versioned APIs and keep saga operations accessible via `OrdersController` for testing and compensating actions.

## Consequences
- Reliable delivery is achieved via durable storage; restarting the worker replays pending events.
- Duplicate deliveries are safely ignored because `ProcessedMessages` is checked before applying side effects.
- Additional infrastructure (DB tables, worker process, RabbitMQ) is required, but the architecture now matches the required reliability guarantees for Submission 3.
