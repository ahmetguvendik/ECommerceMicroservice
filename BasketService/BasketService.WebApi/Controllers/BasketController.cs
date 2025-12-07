using MediatR;
using Microsoft.AspNetCore.Mvc;
using BasketService.Application.Features.Commands.BasketCommands;

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
    public async Task<IActionResult> Create([FromBody] CreateBasketCommand command)
    {
        await _mediator.Send(command);
        return Ok();
    }

    [HttpPost("add-item")]
    public async Task<IActionResult> AddItem([FromBody] AddItemToBasketCommand command)
    {
        await _mediator.Send(command);
        return Ok();
    }
}

