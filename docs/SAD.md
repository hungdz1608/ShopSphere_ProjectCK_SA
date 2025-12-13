# ShopSphere — Software Architecture Document (Submission 1)

## 1. System Overview
ShopSphere là backend cho nền tảng e-commerce, cung cấp các chức năng chính: Catalog (sản phẩm, danh mục), Cart, Orders và Payments. Hệ thống được thiết kế theo clean architecture và modular monolith để đảm bảo dễ bảo trì và phát triển theo từng milestone.

Trong Submission 1, phạm vi triển khai tập trung vào module Catalog:
- Thiết kế nền tảng kiến trúc.
- Xây dựng data model & lớp dữ liệu.
- Triển khai 2 CRUD APIs cốt lõi: Product và Category.

---

## 2. Quality Attributes

### 2.1 Performance
- Catalog là luồng đọc-heavy → API list/search sản phẩm cần phản hồi nhanh.
- Mục tiêu ban đầu (chưa cache):
  - P95 `GET /products` < 300ms với dataset tầm vài nghìn sản phẩm.
- Thiết kế query hỗ trợ filtering/sorting hiệu quả và tránh N+1.

### 2.2 Scalability
- Hệ thống bắt đầu với modular monolith để team phát triển nhanh.
- Module boundaries rõ ràng (Catalog/Cart/Order/Payment).
- Chuẩn bị sẵn cho scale-out bằng caching (Redis) và async messaging (RabbitMQ) ở Sub2.
- Hướng tới event-driven + outbox + saga ở Sub3.

### 2.3 Reliability
- CRUD catalog sử dụng transaction đảm bảo tính nhất quán.
- Thiết kế data layer rõ ràng, tránh lỗi cập nhật chồng chéo.
- Reliability của checkout sẽ được tăng cường bằng Outbox pattern ở Submission 3.

### 2.4 Maintainability
- Áp dụng Clean Architecture:
  - API layer không truy cập DB trực tiếp.
  - Application layer chứa use-cases/services.
  - Domain layer chứa entities và rule nghiệp vụ.
  - Infrastructure layer triển khai EF Core, repositories.
- Quy ước folder tách biệt, giúp code dễ đọc và test được.

---

## 3. C4 Context Diagram (Level 1)
Actors & external systems:

- **User/Guest**: duyệt catalog, (sau này) mua hàng.
- **Admin**: quản lý product/category.
- **Staff**: (sau này) xử lý đơn.
- **Payment Gateway (Mock)**: xử lý thanh toán (Sub3).
- **Notification Service (Mock)**: gửi thông báo đơn hàng (Sub3).

```mermaid
flowchart LR
  User[User/Guest] -->|Browse products| ShopSphere[ShopSphere Backend]
  Admin[Admin] -->|Manage catalog| ShopSphere
  Staff[Staff] -->|Process orders| ShopSphere

  ShopSphere --> Pay[Payment Gateway (Mock)]
  ShopSphere --> Noti[Email/SMS Service (Mock)]
```

---

## 4. C4 Container Diagram (Level 2)
Containers chính của hệ thống:

1. **ShopSphere Web API (.NET 9)**
   - REST endpoints cho Catalog (Sub1).
   - Sẽ mở rộng cho Cart/Order/Payment.

2. **SQL Server Database**
   - Lưu dữ liệu Product, Category.
   - Sẽ mở rộng sang Order/Payment/Outbox tables.

3. **Redis Cache (Sub2)**
   - Cache các endpoint đọc-heavy của catalog.

4. **RabbitMQ (Sub2)**
   - Event bus cho các domain events.

5. **Worker Service (Sub3)**
   - Publish outbox events.
   - Consumers + Saga coordination.

```mermaid
flowchart TB
  Client[Web/Mobile/Admin Client] --> API[ShopSphere Web API\nASP.NET Core]
  API --> DB[(SQL Server)]
  API -.cache(Sub2).-> Redis[(Redis)]
  API -.events(Sub2).-> MQ[(RabbitMQ)]
  Worker[Background Worker\n(Sub3)] -.consume/publish.-> MQ
  Worker --> DB
```

---

## 5. Architecture Strategy

### 5.1 Layered Clean Architecture
- **Domain**: Entities (Product, Category), enums, domain rules.
- **Application**: Services/use-cases, interfaces cho repositories.
- **Infrastructure**: EF Core DbContext, repository implementations.
- **API**: Controllers, request/response contracts.

### 5.2 Module Boundaries (Modular Monolith)
- **Catalog Module (Sub1)**:
  - Product, Category, Inventory (inventory triển khai sau).
- **Cart Module (Sub3)**:
  - Cart, CartItem.
- **Order Module (Sub3)**:
  - Order, OrderItem.
- **Payment Module (Sub3)**:
  - Payment.
- **Async/Reliability (Sub2–3)**:
  - RabbitMQ, Outbox, Saga.

Mỗi module được phát triển độc lập trong cùng monolith, đảm bảo không phụ thuộc ngược vào Infrastructure.

---

## 6. Domain Model Summary (Submission 1)

### 6.1 Entities
**Category**
- id, name, slug
- parentId (nullable)
- createdAt, updatedAt

**Product**
- id, name, slug, description
- price (decimal)
- status (ACTIVE/INACTIVE)
- categoryId (FK)
- createdAt, updatedAt

### 6.2 Relationships
- Category 1–N Product.
- Category có quan hệ parent-child (self reference).

ERD chi tiết được cung cấp trong `docs/erd.png`.

---

## 7. Future Evolution (Submission 2–3)
- **Submission 2:** thêm JWT authentication + role-based authorization ở API layer; thêm Redis caching cho các endpoint đọc-heavy của Catalog; tích hợp RabbitMQ và publish các domain events cơ bản (ví dụ ProductCreated/ProductUpdated).
- **Submission 3:** triển khai **Outbox pattern** trong Infrastructure để đảm bảo event publishing tin cậy; tách **Worker Service** để publish outbox và chạy consumers; bổ sung **Idempotency** cho consumers; xây dựng **Saga checkout** điều phối nhiều bước Order → Payment → Confirm/Cancel; áp dụng API versioning `/v1`.

---

## 8. Key Architectural Decisions (Summary)
- Chọn stack **.NET 9 + EF Core + SQL Server + Redis + RabbitMQ** để phù hợp yêu cầu backend, caching, async và reliability.
- Áp dụng **Clean Architecture + Modular Monolith** để phát triển nhanh trong Sub1, nhưng vẫn giữ ranh giới module rõ ràng cho event-driven evolution ở Sub2–3.

---

## 9. Submission 1 Deliverables Mapping
- Vision & Scope: `/deliverables/submission1/Vision-Scope.md`
- SAD Context + Container + Quality: `/docs/SAD.md`
- ERD: `/docs/erd.png`
- Working CRUD code:
  - Product CRUD
  - Category CRUD

---

## 10. Submission 3 Additions (Reliability & Async)
- **API versioning**: all controllers annotated with `[ApiVersion("1.0")]` and routes are served under `/v1/*`.
- **Outbox pattern**: `OutboxEventPublisher` writes events into `OutboxMessages` (same DB transaction as domain writes). Worker polls and publishes to RabbitMQ.
- **Saga flow**: checkout creates `Order` + `Payment` + `order.created` event; worker consumer finishes payment and emits `payment.completed`.
- **Idempotency**: `ProcessedMessages` tracks handled RabbitMQ `MessageId` values to skip duplicates.
- **Background worker**: `OutboxDispatcher` + `OrderPaymentConsumer` run in `ShopSphere.Worker` project, sharing the same SQL + RabbitMQ configuration.
