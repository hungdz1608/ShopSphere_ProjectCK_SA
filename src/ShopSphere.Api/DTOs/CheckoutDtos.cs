namespace ShopSphere.Api.DTOs;

public record CheckoutRequestDto(string BuyerEmail, decimal TotalAmount);
public record CheckoutResponseDto(Guid OrderId, string BuyerEmail, decimal TotalAmount, string Status);
public record PaymentResultDto(Guid OrderId, string Status, string Message);
