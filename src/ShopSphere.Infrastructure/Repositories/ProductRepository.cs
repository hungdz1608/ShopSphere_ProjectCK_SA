using Microsoft.EntityFrameworkCore;
using ShopSphere.Application.Repositories;
using ShopSphere.Domain.Entities;
using ShopSphere.Infrastructure.db;

namespace ShopSphere.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly ShopSphereDbContext _db;
    public ProductRepository(ShopSphereDbContext db) => _db = db;

    public async Task<Product> AddAsync(Product product)
    {
        _db.Products.Add(product);
        await _db.SaveChangesAsync();
        return product;
    }

    public Task<Product?> GetByIdAsync(Guid id)
        => _db.Products.FirstOrDefaultAsync(p => p.Id == id);

    public Task<Product?> GetBySlugAsync(string slug)
        => _db.Products.FirstOrDefaultAsync(p => p.Slug == slug);

    public async Task<(List<Product> Items, int Total)> GetPagedFilteredAsync(
        int page, int pageSize,
        string? search, Guid? categoryId,
        decimal? minPrice, decimal? maxPrice,
        string? sort)
    {
        var query = _db.Products.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var k = search.Trim().ToLower();
            query = query.Where(p =>
                p.Name.ToLower().Contains(k) ||
                p.Slug.ToLower().Contains(k));
        }

        if (categoryId.HasValue)
            query = query.Where(p => p.CategoryId == categoryId.Value);

        if (minPrice.HasValue)
            query = query.Where(p => p.Price >= minPrice.Value);

        if (maxPrice.HasValue)
            query = query.Where(p => p.Price <= maxPrice.Value);

        query = sort switch
        {
            "price_asc" => query.OrderBy(p => p.Price),
            "price_desc" => query.OrderByDescending(p => p.Price),
            _ => query.OrderByDescending(p => p.CreatedAt) // newest default
        };

        var total = await query.CountAsync();

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task UpdateAsync(Product product)
    {
        _db.Products.Update(product);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Product product)
    {
        _db.Products.Remove(product);
        await _db.SaveChangesAsync();
    }
}
