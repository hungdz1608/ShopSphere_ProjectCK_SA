# ADR-002: Clean Architecture + Modular Monolith

## Context
Hệ thống ShopSphere sẽ tiến hóa qua 3 submission: từ nền tảng monolith đến scalable/event-driven. Team 3 người cần một kiến trúc:
- Dễ phát triển nhanh trong Sub1
- Không vỡ khi thêm auth/cache/async ở Sub2–3
- Có ranh giới module rõ ràng

## Decision
Áp dụng:
1. **Clean Architecture (layered)**
2. **Modular Monolith theo domain modules**

## Rationale
- Clean Architecture giúp tách rõ domain/application/infrastructure, dễ test và bảo trì.
- Modular monolith cho phép phát triển nhanh trong 8 tuần mà không cần overhead microservices.
- Ranh giới module (Catalog/Cart/Order/Payment) rõ giúp chuyển sang event-driven dễ dàng.
- Outbox/saga ở Sub3 có thể thêm vào Infrastructure/Worker mà không ảnh hưởng Domain core.

## Consequences
- Controllers không được truy cập DbContext trực tiếp.
- Application chỉ giao tiếp với Infrastructure qua interfaces.
- Các module phải tránh circular dependencies.
- Khi thêm async, event contracts phải giữ ổn định để không phá use-cases.
