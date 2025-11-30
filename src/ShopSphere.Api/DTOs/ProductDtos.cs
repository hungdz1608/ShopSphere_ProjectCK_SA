namespace ShopSphere.Api.DTOs;

public record CreateProductDto(string Name, decimal Price, int Stock, Guid CategoryId);
public record UpdateProductDto(string Name, decimal Price, int Stock, Guid CategoryId);

public record ProductQueryDto(
    int Page = 1,
    int PageSize = 10,
    string? Search = null,
    Guid? CategoryId = null,
    decimal? MinPrice = null,
    decimal? MaxPrice = null,
    string? Sort = null // price_asc | price_desc | newest
);
