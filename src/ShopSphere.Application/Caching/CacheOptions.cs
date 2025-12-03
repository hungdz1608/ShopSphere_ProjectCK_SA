namespace ShopSphere.Application.Caching;

public class CacheOptions
{
    public int ListTtlSeconds { get; set; } = 60;
    public int ItemTtlSeconds { get; set; } = 120;
}
