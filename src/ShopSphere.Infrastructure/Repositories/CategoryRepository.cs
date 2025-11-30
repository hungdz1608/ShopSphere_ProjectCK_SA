using Microsoft.EntityFrameworkCore;
using ShopSphere.Application.Repositories;
using ShopSphere.Domain.Entities;
using ShopSphere.Infrastructure.db;

namespace ShopSphere.Infrastructure.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly ShopSphereDbContext _db;
    public CategoryRepository(ShopSphereDbContext db) => _db = db;

    public async Task<Category> AddAsync(Category category)
    {
        _db.Categories.Add(category);
        await _db.SaveChangesAsync();
        return category;
    }

    public Task<Category?> GetByIdAsync(Guid id)
        => _db.Categories.FirstOrDefaultAsync(c => c.Id == id);

    public Task<Category?> GetBySlugAsync(string slug)
        => _db.Categories.FirstOrDefaultAsync(c => c.Slug == slug);

    public async Task<(List<Category> Items, int Total)> GetPagedAsync(int page, int pageSize, string? q)
    {
        var query = _db.Categories.AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var k = q.Trim().ToLower();
            query = query.Where(c =>
                c.Name.ToLower().Contains(k) ||
                c.Slug.ToLower().Contains(k));
        }

        var total = await query.CountAsync();

        var items = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task UpdateAsync(Category category)
    {
        _db.Categories.Update(category);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Category category)
    {
        _db.Categories.Remove(category);
        await _db.SaveChangesAsync();
    }
}
