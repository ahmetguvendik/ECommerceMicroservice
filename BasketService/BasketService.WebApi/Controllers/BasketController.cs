using MediatR;
using Microsoft.AspNetCore.Mvc;
using BasketService.Application.Features.Commands.BasketCommands;
using BasketService.Application.Features.Queries.BasketQueries;

namespace BasketService.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BasketController : ControllerBase
{
    private readonly IMediator _mediator;

    public BasketController(IMediator mediator)
    {
         _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> AddItem([FromBody] AddItemToBasketCommand command)
    {
        await _mediator.Send(command);
        return Ok();
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetBasketByIdQuery(id));
        
        if (result == null)
            return NotFound($"Basket with Id {id} not found.");
        
        return Ok(result);
    }

    [HttpGet("customer/{customerId}")]
    public async Task<IActionResult> GetByCustomerId(Guid customerId)
    {
        var result = await _mediator.Send(new GetBasketByCustomerIdQuery(customerId));
        
        if (result == null)
            return NotFound($"Basket for CustomerId {customerId} not found.");
        
        return Ok(result);
    }

    [HttpPost("checkout")]
    public async Task<IActionResult> Checkout([FromBody] CheckoutBasketCommand command)
    {
        var orderId = await _mediator.Send(command);
        return Accepted(new { orderId });
    }
}

