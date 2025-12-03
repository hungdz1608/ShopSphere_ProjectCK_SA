namespace ShopSphere.Api.DTOs;

public record LoginRequestDto(string Username, string Password);

public record LoginResponseDto(string AccessToken, string Role, DateTime ExpiresAtUtc);
