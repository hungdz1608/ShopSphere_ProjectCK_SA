using Microsoft.Extensions.Options;
using ShopSphere.Application.Authentication;

namespace ShopSphere.Infrastructure.Auth;

public class InMemoryUserStore : IUserStore
{
    private readonly List<AuthUserConfig> _users;

    public InMemoryUserStore(IOptions<AuthOptions> options)
    {
        _users = options.Value.Users.Select(u => new AuthUserConfig
        {
            Username = u.Username,
            Password = u.Password,
            Role = u.Role
        }).ToList();
    }

    public Task<AuthUserConfig?> FindByUsernameAsync(string username)
    {
        var user = _users.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(user);
    }
}
