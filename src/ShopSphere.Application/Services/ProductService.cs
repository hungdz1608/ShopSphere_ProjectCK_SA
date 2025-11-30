using ShopSphere.Application.Repositories;
using ShopSphere.Domain.Entities;
using ShopSphere.Domain.Errors;
using System.Text.RegularExpressions;

namespace ShopSphere.Application.Services;

public class ProductService
{
    private readonly IProductRepository _productRepo;
    private readonly ICategoryRepository _categoryRepo;

    public ProductService(IProductRepository productRepo, ICategoryRepository categoryRepo)
    {
        _productRepo = productRepo;
        _categoryRepo = categoryRepo;
    }

    private static string Slugify(string input)
    {
        var slug = input.Trim().ToLower();
        slug = Regex.Replace(slug, @"\s+", "-");
        slug = Regex.Replace(slug, @"[^a-z0-9\-]", "");
        return slug;
    }

    public async Task<Product> CreateAsync(string name, decimal price, int stock, Guid categoryId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new BadRequestException("Name is required.");
        if (price <= 0)
            throw new BadRequestException("Price must be > 0.");

        var cat = await _categoryRepo.GetByIdAsync(categoryId);
        if (cat == null)
            throw new BadRequestException("CategoryId does not exist.");

        var slug = Slugify(name);
        var exists = await _productRepo.GetBySlugAsync(slug);
        if (exists != null)
            throw new BadRequestException("Slug already exists.");

        var product = new Product
        {
            Name = name.Trim(),
            Slug = slug,
            Price = price,
            Stock = stock,
            CategoryId = categoryId
        };

        return await _productRepo.AddAsync(product);
    }

    public async Task<(List<Product> Items, int Total)> GetPagedFilteredAsync(
        int page, int pageSize, string? search, Guid? categoryId,
        decimal? minPrice, decimal? maxPrice, string? sort)
    {
        page = page <= 0 ? 1 : page;
        pageSize = pageSize <= 0 ? 10 : pageSize;
        return await _productRepo.GetPagedFilteredAsync(
            page, pageSize, search, categoryId, minPrice, maxPrice, sort
        );
    }

    public async Task<Product> GetByIdAsync(Guid id)
    {
        var p = await _productRepo.GetByIdAsync(id);
        if (p == null) throw new NotFoundException("Product not found.");
        return p;
    }

    public async Task<Product> UpdateAsync(Guid id, string name, decimal price, int stock, Guid categoryId)
    {
        var p = await GetByIdAsync(id);

        if (string.IsNullOrWhiteSpace(name))
            throw new BadRequestException("Name is required.");
        if (price <= 0)
            throw new BadRequestException("Price must be > 0.");

        var cat = await _categoryRepo.GetByIdAsync(categoryId);
        if (cat == null)
            throw new BadRequestException("CategoryId does not exist.");

        var newSlug = Slugify(name);
        var slugOwner = await _productRepo.GetBySlugAsync(newSlug);
        if (slugOwner != null && slugOwner.Id != id)
            throw new BadRequestException("Slug already exists.");

        p.Name = name.Trim();
        p.Slug = newSlug;
        p.Price = price;
        p.Stock = stock;
        p.CategoryId = categoryId;
        p.UpdatedAt = DateTime.UtcNow;

        await _productRepo.UpdateAsync(p);
        return p;
    }

    public async Task DeleteAsync(Guid id)
    {
        var p = await GetByIdAsync(id);
        await _productRepo.DeleteAsync(p);
    }
}
