using ShopSphere.Domain.Entities;

namespace ShopSphere.Application.Repositories;

public interface IProductRepository
{
    Task<Product> AddAsync(Product product);
    Task<Product?> GetByIdAsync(Guid id);
    Task<Product?> GetBySlugAsync(string slug);
    Task<(List<Product> Items, int Total)> GetPagedFilteredAsync(
        int page, int pageSize,
        string? search, Guid? categoryId,
        decimal? minPrice, decimal? maxPrice,
        string? sort
    );
    Task UpdateAsync(Product product);
    Task DeleteAsync(Product product);
}
