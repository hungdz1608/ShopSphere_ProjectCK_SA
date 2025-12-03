# Caching Plan (Submission 2)

## Layers
1. **In-memory application cache**: IMemoryCache caches catalog queries and detail lookups.
2. **HTTP caching**: ETag + `Cache-Control` headers allow clients/CDNs to reuse responses.

## Cache keys & TTLs
- TTLs are configurable in `Cache:ListTtlSeconds` (default 60s) and `Cache:ItemTtlSeconds` (default 120s).
- Keys include a per-entity **cache token** to invalidate all entries after writes:
  - Categories list: `categories:{token}:page={page}:size={pageSize}:q={query}`
  - Category detail: `category:{token}:id={id}`
  - Products list: `products:{token}:p={page}:s={pageSize}:search={search}:cat={categoryId}:min={minPrice}:max={maxPrice}:sort={sort}`
  - Product detail: `product:{token}:id={id}`

## Invalidation strategy
- Each write (create/update/delete) regenerates the cache token for its entity type, invalidating all list/detail entries in one step.
- Short TTLs limit stale data exposure between token rotations.

## HTTP cache headers
- Responses include `ETag` computed from serialized payloads; clients sending the same `If-None-Match` receive `304 Not Modified`.
- `Cache-Control` set to `public, max-age=<ttl>` mirrors the in-memory cache durations for consistency.

## Future improvements
- Swap IMemoryCache with Redis for multi-instance deployments.
- Use change tokens tied to database notifications to shrink staleness windows.
