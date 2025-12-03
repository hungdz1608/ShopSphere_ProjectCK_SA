namespace ShopSphere.Application.Authentication;

public interface IUserStore
{
    Task<AuthUserConfig?> FindByUsernameAsync(string username);
}
