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
