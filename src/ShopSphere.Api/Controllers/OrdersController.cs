using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopSphere.Api.DTOs;
using ShopSphere.Application.Services;
using ShopSphere.Domain.Errors;

namespace ShopSphere.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("v1/orders")]
public class OrdersController : ControllerBase
{
    private readonly CheckoutService _checkoutService;

    public OrdersController(CheckoutService checkoutService)
    {
        _checkoutService = checkoutService;
    }

    [Authorize(Policy = "RequireAuthenticated")]
    [HttpPost("checkout")]
    public async Task<IActionResult> Checkout([FromBody] CheckoutRequestDto dto, CancellationToken cancellationToken)
    {
        var order = await _checkoutService.StartCheckoutAsync(dto.BuyerEmail, dto.TotalAmount, cancellationToken);
        return Accepted(new CheckoutResponseDto(order.Id, order.BuyerEmail, order.TotalAmount, order.Status.ToString()));
    }

    [Authorize(Policy = "RequireAuthenticated")]
    [HttpPost("{id:guid}/payment/confirm")]
    public async Task<IActionResult> Confirm(Guid id, CancellationToken cancellationToken)
    {
        var order = await _checkoutService.MarkPaymentCompletedAsync(id, cancellationToken);
        return Ok(new PaymentResultDto(order.Id, order.Status.ToString(), "Payment confirmed"));
    }

    [Authorize(Policy = "RequireAuthenticated")]
    [HttpPost("{id:guid}/payment/fail")]
    public async Task<IActionResult> Fail(Guid id, [FromBody] string reason, CancellationToken cancellationToken)
    {
        var order = await _checkoutService.FailPaymentAsync(id, reason, cancellationToken);
        return Ok(new PaymentResultDto(order.Id, order.Status.ToString(), reason));
    }

    [Authorize(Policy = "RequireAuthenticated")]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var order = await _checkoutService.GetAsync(id, cancellationToken);
            return Ok(new CheckoutResponseDto(order.Id, order.BuyerEmail, order.TotalAmount, order.Status.ToString()));
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
    }
}
