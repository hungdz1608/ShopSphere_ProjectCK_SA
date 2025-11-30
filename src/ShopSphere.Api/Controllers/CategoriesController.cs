using Microsoft.AspNetCore.Mvc;
using ShopSphere.Api.DTOs;
using ShopSphere.Application.Services;

namespace ShopSphere.Api.Controllers;

[ApiController]
[Route("categories")]
public class CategoriesController : ControllerBase
{
    private readonly CategoryService _service;
    public CategoriesController(CategoryService service) => _service = service;

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCategoryDto dto)
    {
        var c = await _service.CreateAsync(dto.Name);
        return CreatedAtAction(nameof(GetById), new { id = c.Id }, c);
    }

    [HttpGet]
    public async Task<IActionResult> GetPaged([FromQuery] CategoryQueryDto q)
    {
        var (items, total) = await _service.GetPagedAsync(q.Page, q.PageSize, q.Q);
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
        var c = await _service.GetByIdAsync(id);
        return Ok(c);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCategoryDto dto)
    {
        var c = await _service.UpdateAsync(id, dto.Name);
        return Ok(c);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }
}
