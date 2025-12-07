using MediatR;
using Microsoft.AspNetCore.Mvc;
using ProductService.Application.Features.Commands.ProductCommands;
using ProductService.Application.UnitOfWorks;
using ProductService.Domain.Entities;

namespace ProductService.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductController(IMediator mediator)
    {
         _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductCommand  command)
    {
        await  _mediator.Send(command);
        return Ok();
    }
    
}