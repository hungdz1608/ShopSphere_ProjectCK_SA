
---

## 3) `/docs/ADRs/ADR-001-stack-choice.md`

```md
# ADR-001: Technology Stack Choice

## Context
Project yêu cầu xây dựng backend large-scale, có authentication, caching và asynchronous messaging trong các milestone sau. Team cần stack phổ biến, dễ phát triển nhanh, hỗ trợ tốt cho Clean Architecture, ORM, caching và message broker.

## Decision
Chọn stack:
- **.NET 9 (ASP.NET Core Web API)**
- **EF Core**
- **SQL Server**
- **Redis**
- **RabbitMQ**

## Rationale
- ASP.NET Core Web API hỗ trợ REST chuẩn, middleware mạnh, dễ tách layer.
- EF Core tích hợp tốt với SQL Server, migration nhanh, phù hợp cho schema evolving theo milestone.
- Redis là lựa chọn chuẩn cho cache read-heavy catalog.
- RabbitMQ phù hợp cho event-driven, hỗ trợ retry/DLQ rõ ràng, dễ làm outbox/saga ở Sub3.
- Stack này đáp ứng đúng focus của đề (Backend, Auth, Data, Caching, Async Messaging).

## Consequences
- Team cần tuân thủ strict layering để tránh coupling vào EF trong domain/application.
- Cần docker-compose cho local dev gồm SQL Server, Redis, RabbitMQ.
- Sau Sub1, phải bổ sung auth + cache + MQ mà không phá vỡ kiến trúc clean đã chọn.
