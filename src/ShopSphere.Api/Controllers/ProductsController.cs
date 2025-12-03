using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using ShopSphere.Api.DTOs;
using ShopSphere.Application.Caching;
using ShopSphere.Application.Services;

namespace ShopSphere.Api.Controllers;

[ApiController]
[Route("products")]
public class ProductsController : ControllerBase
{
    private readonly ProductService _service;
    private readonly IMemoryCache _cache;
    private readonly CacheOptions _cacheOptions;
    private const string CacheTokenKey = "products-cache-token";

    public ProductsController(ProductService service, IMemoryCache cache, IOptions<CacheOptions> cacheOptions)
    {
        _service = service;
        _cache = cache;
        _cacheOptions = cacheOptions.Value;
    }

    private string GetCacheToken() => _cache.GetOrCreate(CacheTokenKey, entry => Guid.NewGuid().ToString())!;
    private void BustCacheToken() => _cache.Set(CacheTokenKey, Guid.NewGuid().ToString());
    private static string ComputeEtag(object data)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(data);
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(json));
        return Convert.ToHexString(hash);
    }

    [Authorize(Policy = "RequireAdminOrStaff")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductDto dto)
    {
        var p = await _service.CreateAsync(dto.Name, dto.Price, dto.Stock, dto.CategoryId);
        BustCacheToken();
        return CreatedAtAction(nameof(GetById), new { id = p.Id }, p);
    }

    [Authorize(Policy = "RequireAuthenticated")]
    [HttpGet]
    public async Task<IActionResult> GetPagedFiltered([FromQuery] ProductQueryDto q)
    {
        var cacheKey = $"products:{GetCacheToken()}:p={q.Page}:s={q.PageSize}:search={q.Search}:cat={q.CategoryId}:min={q.MinPrice}:max={q.MaxPrice}:sort={q.Sort}";
        var cached = await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(_cacheOptions.ListTtlSeconds);
            var (items, total) = await _service.GetPagedFilteredAsync(
                q.Page, q.PageSize, q.Search, q.CategoryId, q.MinPrice, q.MaxPrice, q.Sort
            );

            return new
            {
                items,
                page = q.Page,
                pageSize = q.PageSize,
                totalItems = total,
                totalPages = (int)Math.Ceiling(total / (double)q.PageSize)
            };
        });

        var etag = ComputeEtag(cached!);
        if (Request.Headers.IfNoneMatch == etag)
            return StatusCode(304);

        Response.Headers.ETag = etag;
        Response.Headers.CacheControl = $"public, max-age={_cacheOptions.ListTtlSeconds}";
        return Ok(cached);
    }

    [Authorize(Policy = "RequireAuthenticated")]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var cacheKey = $"product:{GetCacheToken()}:id={id}";
        var cached = await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(_cacheOptions.ItemTtlSeconds);
            return await _service.GetByIdAsync(id);
        });

        var etag = ComputeEtag(cached!);
        if (Request.Headers.IfNoneMatch == etag)
            return StatusCode(304);

        Response.Headers.ETag = etag;
        Response.Headers.CacheControl = $"public, max-age={_cacheOptions.ItemTtlSeconds}";
        return Ok(cached);
    }

    [Authorize(Policy = "RequireAdminOrStaff")]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductDto dto)
    {
        var p = await _service.UpdateAsync(id, dto.Name, dto.Price, dto.Stock, dto.CategoryId);
        BustCacheToken();
        return Ok(p);
    }

    [Authorize(Policy = "RequireAdminOrStaff")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _service.DeleteAsync(id);
        BustCacheToken();
        return NoContent();
    }
}
