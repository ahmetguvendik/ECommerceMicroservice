using MediatR;
using Microsoft.AspNetCore.Mvc;
using ProductService.Application.Features.Commands.ProductCategoryCommands;

namespace ProductService.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoryController : ControllerBase
{
    private readonly IMediator _mediator;

    public CategoryController(IMediator mediator)
    {
         _mediator = mediator;
    }
    
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductCategoryCommand  command)
    {
        await  _mediator.Send(command);
        return Ok();
    }
    
}