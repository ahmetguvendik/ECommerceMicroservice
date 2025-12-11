using MediatR;
using Microsoft.AspNetCore.Mvc;
using ProductService.Application.Features.Commands.ProductCommands;
using ProductService.Application.Features.Queries.ProductQueries;

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

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateProductCommand command)
    {
        await  _mediator.Send(command);
        return Ok();
    }

    [HttpDelete]
    public async Task<IActionResult> Delete([FromBody] DeleteProductCommand command)
    {
        await  _mediator.Send(command);
        return Ok();
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetProductByIdQuery(id));
        
        if (result == null)
            return NotFound($"Product with Id {id} not found.");
        
        return Ok(result);
    }
    
}