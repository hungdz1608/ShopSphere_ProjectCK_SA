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
[ApiVersion("1.0")]
[Route("v1/categories")]
public class CategoriesController : ControllerBase
{
    private readonly CategoryService _service;
    private readonly IMemoryCache _cache;
    private readonly CacheOptions _cacheOptions;
    private const string CacheTokenKey = "categories-cache-token";

    public CategoriesController(CategoryService service, IMemoryCache cache, IOptions<CacheOptions> cacheOptions)
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
    public async Task<IActionResult> Create([FromBody] CreateCategoryDto dto)
    {
        var c = await _service.CreateAsync(dto.Name);
        BustCacheToken();
        return CreatedAtAction(nameof(GetById), new { id = c.Id }, c);
    }

    [Authorize(Policy = "RequireAuthenticated")]
    [HttpGet]
    public async Task<IActionResult> GetPaged([FromQuery] CategoryQueryDto q)
    {
        var cacheKey = $"categories:{GetCacheToken()}:page={q.Page}:size={q.PageSize}:q={q.Q}";
        var cached = await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(_cacheOptions.ListTtlSeconds);
            var (items, total) = await _service.GetPagedAsync(q.Page, q.PageSize, q.Q);
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
        var cacheKey = $"category:{GetCacheToken()}:id={id}";
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
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCategoryDto dto)
    {
        var c = await _service.UpdateAsync(id, dto.Name);
        BustCacheToken();
        return Ok(c);
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
