# ShopSphere — System Vision & Scope (Submission 1)

## 1. Vision
ShopSphere là một nền tảng thương mại điện tử (e-commerce) hướng tới trải nghiệm mua sắm nhanh, ổn định và có khả năng mở rộng. Hệ thống cung cấp catalog sản phẩm phong phú, hỗ trợ giỏ hàng, đặt hàng và thanh toán. Trong bối cảnh tải truy cập tăng đột biến theo mùa hoặc chiến dịch marketing, ShopSphere được thiết kế theo hướng modular và sẵn sàng tiến hóa sang kiến trúc event-driven để đảm bảo hiệu năng cũng như độ tin cậy.

Ở giai đoạn Submission 1, ShopSphere tập trung xây dựng nền tảng kiến trúc và lớp dữ liệu cho module Catalog, thiết lập các quyết định kiến trúc cốt lõi và triển khai các API CRUD cơ bản làm cơ sở cho những milestone tiếp theo.

---

## 2. Goals

### 2.1 Business Goals
- Cho phép người dùng duyệt, tìm kiếm và lọc sản phẩm theo category/keyword/giá.
- Cho phép admin quản lý danh mục (category) và sản phẩm (product).
- Tạo nền hoạt động ổn định để phát triển giỏ hàng, đặt hàng và thanh toán ở các giai đoạn sau.

### 2.2 Technical Goals
- Thiết kế backend theo clean architecture / modular monolith.
- Data model rõ ràng, có ERD và migration.
- API tuân thủ REST, có pagination và filtering cho các endpoint đọc.
- Chuẩn bị sẵn nền tảng để bổ sung authentication, caching, async messaging, outbox/saga ở Submission 2 và 3.

---

## 3. Stakeholders & Users

### 3.1 User/Guest (Khách mua hàng)
- Duyệt catalog sản phẩm.
- Tìm kiếm/lọc sản phẩm theo nhu cầu.
- (Giai đoạn sau) thêm giỏ hàng, checkout, thanh toán.

### 3.2 Admin (Quản trị)
- Thêm/sửa/xóa sản phẩm.
- Thêm/sửa/xóa category, quản lý cấu trúc danh mục.

### 3.3 Staff (Nhân viên vận hành)
- (Giai đoạn sau) xử lý đơn hàng, cập nhật trạng thái giao hàng.

---

## 4. MVP — Submission 1 Scope
Submission 1 chỉ tập trung vào **Foundational Architecture** và module Catalog:

### 4.1 Catalog Foundation
- Category:
  - Tạo danh mục sản phẩm với quan hệ parent-child đơn giản.
- Product:
  - Tạo sản phẩm có thông tin cơ bản (name, description, price, status, category).

### 4.2 Core CRUD APIs
Triển khai 2 nhóm CRUD endpoints cho:
- Category CRUD
- Product CRUD

**Yêu cầu cho API đọc (GET list):**
- Pagination: page, pageSize
- Filtering:
  - Product: search, categoryId, minPrice, maxPrice
- Sorting:
  - price_asc, price_desc, newest

### 4.3 Architecture & Data Layer
- C4 Context diagram.
- C4 Container diagram.
- Thiết kế ERD cho Product–Category.
- ORM mapping bằng EF Core.
- Migrations chạy được trên SQL Server.

---

## 5. Out of Scope — Submission 1
Các chức năng sau **không nằm trong Submission 1**:
- Cart, Order, Payment flows.
- Authentication/Authorization (JWT/OIDC).
- Redis caching và HTTP caching.
- Async messaging (RabbitMQ).
- Outbox pattern, Saga orchestration, Idempotency.

Chúng sẽ được thực hiện ở Submission 2 và Submission 3.

---

## 6. Assumptions & Constraints
- Hệ thống phục vụ cho một storefront (single tenant).
- Sử dụng cơ sở dữ liệu quan hệ (SQL Server).
- API backend là nguồn dữ liệu chính, client không truy cập DB trực tiếp.
- Các dịch vụ ngoài (payment/notification) ở giai đoạn sau có thể dùng mock.

---

## 7. Success Criteria (Submission 1)
Submission 1 được xem là hoàn thành khi:
- Có Vision & Scope document (2–3 trang).
- Có SAD.md với Context + Container diagrams và quality goals.
- Có ERD (erd.png) đúng nghiệp vụ.
- Code chạy được:
  - Category CRUD.
  - Product CRUD.
  - GET list hỗ trợ pagination, filtering, sorting theo yêu cầu.
- Repo build/run được rõ ràng và tái lập được.
