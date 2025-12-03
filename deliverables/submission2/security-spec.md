# Security Specification (Submission 2)

## Authentication flow
- **Protocol**: JWT Bearer tokens signed with HMAC-SHA256.
- **Issuing endpoint**: `POST /auth/login` accepts `{ "username", "password" }` and returns an access token, user role, and expiry.
- **Token claims**: `sub` + `name` (username) and `role` (Admin/Staff/User).
- **Expiry**: configurable via `Auth:AccessTokenMinutes` (default 120 minutes).
- **Validation**: Issuer, audience, and signing key validated by `JwtBearer` middleware.

## Roles and protected endpoints
- **Admin & Staff**: required for write operations on catalog data.
  - `POST /products`, `PUT /products/{id}`, `DELETE /products/{id}`
  - `POST /categories`, `PUT /categories/{id}`, `DELETE /categories/{id}`
- **All authenticated users (Admin/Staff/User)**: may read catalog data.
  - `GET /products`, `GET /products/{id}`
  - `GET /categories`, `GET /categories/{id}`
- Anonymous users are blocked from catalog routes; they must obtain a token first.

## Configuration
- Set under `Auth` in `appsettings*.json`:
  - `Issuer`, `Audience`, `SigningKey` (required), `AccessTokenMinutes`.
  - Demo users with roles for local testing (admin, staff, buyer).

## Error handling & responses
- Invalid credentials → `401 Unauthorized` with `{ message: "Invalid credentials" }`.
- Missing/invalid token → framework-generated 401/403 responses.
- Domain errors still surface as structured 400/404 responses from the global handler.

## Operational notes
- Tokens are included via `Authorization: Bearer <token>`.
- Swagger UI is configured with a Bearer security scheme to simplify manual testing.
