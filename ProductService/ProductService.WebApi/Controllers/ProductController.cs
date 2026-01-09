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
    private readonly ILogger<ProductController> _logger;

    public ProductController(IMediator mediator, ILogger<ProductController> logger)
    {
         _mediator = mediator;
         _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductCommand  command)
    {
        _logger.LogInformation("Creating product: {ProductName}, SKU: {Sku}", command.Name, command.Sku);
        await  _mediator.Send(command);
        _logger.LogInformation("Product created successfully: {ProductName}", command.Name);
        return Ok();
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateProductCommand command)
    {
        _logger.LogInformation("Updating product: {ProductId}", command.Id);
        await  _mediator.Send(command);
        _logger.LogInformation("Product updated successfully: {ProductId}", command.Id);
        return Ok();
    }

    [HttpDelete]
    public async Task<IActionResult> Delete([FromBody] DeleteProductCommand command)
    {
        _logger.LogInformation("Deleting product: {ProductId}", command.Id);
        await  _mediator.Send(command);
        _logger.LogInformation("Product deleted successfully: {ProductId}", command.Id);
        return Ok();
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        _logger.LogInformation("Getting product by ID: {ProductId}", id);
        var result = await _mediator.Send(new GetProductByIdQuery(id));
        
        if (result == null)
        {
            _logger.LogWarning("Product not found: {ProductId}", id);
            return NotFound($"Product with Id {id} not found.");
        }
        
        _logger.LogInformation("Product retrieved successfully: {ProductId}", id);
        return Ok(result);
    }
    
}