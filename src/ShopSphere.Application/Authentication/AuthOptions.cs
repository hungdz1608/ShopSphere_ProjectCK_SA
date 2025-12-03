namespace ShopSphere.Application.Authentication;

public class AuthOptions
{
    public string Issuer { get; set; } = "ShopSphere";
    public string Audience { get; set; } = "ShopSphere";
    public string SigningKey { get; set; } = string.Empty;
    public int AccessTokenMinutes { get; set; } = 60;
    public List<AuthUserConfig> Users { get; set; } = new();
}
