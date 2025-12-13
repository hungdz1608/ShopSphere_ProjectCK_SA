# Consistency & Reliability Plan (Submission 3)

## Outbox pattern
- **Table**: `OutboxMessages` with columns `{ Id (GUID), EventName, Payload (JSON), Status, RetryCount, OccurredAt, PublishedAt, Error }`.
- **Writers**: API layer registers `OutboxEventPublisher` (implements `IEventPublisher`). Domain services enqueue JSON payloads with deterministic IDs (order/payment IDs are reused as `MessageId`).
- **Dispatcher**: `ShopSphere.Worker` runs `OutboxDispatcher` (background service) to poll pending rows every 5 seconds, publish to RabbitMQ, set `PublishedAt`, and increment `RetryCount` when failures occur.
- **Retry/backoff**: transient failures keep the row in `Pending` with incremented `RetryCount`. Operator can restart the worker; the dispatcher replays all `Pending` rows idempotently.

## Idempotent consumers
- **ProcessedMessages** table stores `{ MessageId, ProcessedAt }`.
- **OrderPaymentConsumer** (worker) binds to `order.*` routing keys. Each delivery checks `ProcessedMessages` before handling; duplicates are **acknowledged and skipped**.
- Messages are acknowledged **after** both business logic and `ProcessedMessages` insert succeed to avoid losing events.

## Saga / Process flow
- **Trigger**: `POST /v1/orders/checkout` creates `Order` + `Payment` (Pending) and outbox event `order.created`.
- **Step 1**: Worker publishes `order.created` to RabbitMQ.
- **Step 2**: `OrderPaymentConsumer` receives `order.created` → marks payment as `Completed` and order as `PaymentCompleted` (idempotent) → emits `payment.completed` outbox entry.
- **Step 3**: Dispatcher publishes `payment.completed` for downstream services (notification, fulfillment, etc.).
- **Failure path**: if external validation fails, `/v1/orders/{id}/payment/fail` marks `PaymentFailed` + `Order.Failed` and emits `payment.failed` for compensating actions.

## API versioning
- All HTTP routes are namespaced under `/v1/*` and controllers are annotated with `[ApiVersion("1.0")]`.

## Operational checkpoints
- Ensure DB schema is created (`EnsureCreated` on API startup) so Outbox/Processed tables exist before dispatching.
- Worker and API share the same connection string and RabbitMQ exchange (`shopsphere.events`).
- Queue consumers use `MessageId` from RabbitMQ properties (set by dispatcher) to enforce idempotency.
