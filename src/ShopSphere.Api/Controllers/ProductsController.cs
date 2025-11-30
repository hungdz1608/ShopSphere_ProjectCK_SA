using Microsoft.AspNetCore.Mvc;
using ShopSphere.Api.DTOs;
using ShopSphere.Application.Services;

namespace ShopSphere.Api.Controllers;

[ApiController]
[Route("products")]
public class ProductsController : ControllerBase
{
    private readonly ProductService _service;
    public ProductsController(ProductService service) => _service = service;

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductDto dto)
    {
        var p = await _service.CreateAsync(dto.Name, dto.Price, dto.Stock, dto.CategoryId);
        return CreatedAtAction(nameof(GetById), new { id = p.Id }, p);
    }

    [HttpGet]
    public async Task<IActionResult> GetPagedFiltered([FromQuery] ProductQueryDto q)
    {
        var (items, total) = await _service.GetPagedFilteredAsync(
            q.Page, q.PageSize, q.Search, q.CategoryId, q.MinPrice, q.MaxPrice, q.Sort
        );

        return Ok(new
        {
            items,
            page = q.Page,
            pageSize = q.PageSize,
            totalItems = total,
            totalPages = (int)Math.Ceiling(total / (double)q.PageSize)
        });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var p = await _service.GetByIdAsync(id);
        return Ok(p);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductDto dto)
    {
        var p = await _service.UpdateAsync(id, dto.Name, dto.Price, dto.Stock, dto.CategoryId);
        return Ok(p);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }
}
