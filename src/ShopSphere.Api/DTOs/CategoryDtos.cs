namespace ShopSphere.Api.DTOs;

public record CreateCategoryDto(string Name);
public record UpdateCategoryDto(string Name);

public record CategoryQueryDto(
    int Page = 1,
    int PageSize = 10,
    string? Q = null
);
