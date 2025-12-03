namespace ShopSphere.Application.Authentication;

public interface ITokenService
{
    string GenerateToken(AuthUserConfig user);
}
