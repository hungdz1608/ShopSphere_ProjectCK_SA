using ShopSphere.Application.Repositories;
using ShopSphere.Domain.Entities;
using ShopSphere.Domain.Errors;
using System.Text.RegularExpressions;

namespace ShopSphere.Application.Services;

public class CategoryService
{
    private readonly ICategoryRepository _repo;

    public CategoryService(ICategoryRepository repo) => _repo = repo;

    private static string Slugify(string input)
    {
        var slug = input.Trim().ToLower();
        slug = Regex.Replace(slug, @"\s+", "-");
        slug = Regex.Replace(slug, @"[^a-z0-9\-]", "");
        return slug;
    }

    public async Task<Category> CreateAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new BadRequestException("Name is required.");

        var slug = Slugify(name);
        var exists = await _repo.GetBySlugAsync(slug);
        if (exists != null)
            throw new BadRequestException("Slug already exists.");

        var category = new Category
        {
            Name = name.Trim(),
            Slug = slug
        };

        return await _repo.AddAsync(category);
    }

    public async Task<(List<Category> Items, int Total)> GetPagedAsync(int page, int pageSize, string? q)
    {
        page = page <= 0 ? 1 : page;
        pageSize = pageSize <= 0 ? 10 : pageSize;
        return await _repo.GetPagedAsync(page, pageSize, q);
    }

    public async Task<Category> GetByIdAsync(Guid id)
    {
        var c = await _repo.GetByIdAsync(id);
        if (c == null) throw new NotFoundException("Category not found.");
        return c;
    }

    public async Task<Category> UpdateAsync(Guid id, string name)
    {
        var c = await GetByIdAsync(id);

        if (string.IsNullOrWhiteSpace(name))
            throw new BadRequestException("Name is required.");

        var newSlug = Slugify(name);
        var slugOwner = await _repo.GetBySlugAsync(newSlug);
        if (slugOwner != null && slugOwner.Id != id)
            throw new BadRequestException("Slug already exists.");

        c.Name = name.Trim();
        c.Slug = newSlug;
        c.UpdatedAt = DateTime.UtcNow;

        await _repo.UpdateAsync(c);
        return c;
    }

    public async Task DeleteAsync(Guid id)
    {
        var c = await GetByIdAsync(id);
        await _repo.DeleteAsync(c);
    }
}
