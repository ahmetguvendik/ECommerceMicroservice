using MassTransit.Mediator;
using Microsoft.AspNetCore.Mvc;
using OrderService.Application.Features.Commands;


namespace OrderService.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrderController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrderController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderCommand request)
    {
        await  _mediator.Send(request);
        return Ok();
    }
}



