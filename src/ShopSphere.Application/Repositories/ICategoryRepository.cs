using ShopSphere.Domain.Entities;

namespace ShopSphere.Application.Repositories;

public interface ICategoryRepository
{
    Task<Category> AddAsync(Category category);
    Task<Category?> GetByIdAsync(Guid id);
    Task<Category?> GetBySlugAsync(string slug);
    Task<(List<Category> Items, int Total)> GetPagedAsync(int page, int pageSize, string? q);
    Task UpdateAsync(Category category);
    Task DeleteAsync(Category category);
}
