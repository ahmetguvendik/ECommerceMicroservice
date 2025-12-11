using MediatR;
using Microsoft.AspNetCore.Mvc;
using StockService.Application.Features.Queries.StockQueries;

namespace StockService.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StockController : ControllerBase
{
    private readonly IMediator _mediator;

    public StockController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new GetStockByIdQuery(id);
        var result = await _mediator.Send(query);
        
        if (result == null)
            return NotFound($"Stock with Id {id} not found.");
        
        return Ok(result);
    }

    [HttpGet("product/{productId}")]
    public async Task<IActionResult> GetByProductId(Guid productId)
    {
        var query = new GetStockByProductIdQuery
        {
            ProductId = productId
        };
        var result = await _mediator.Send(query);
        
        if (result == null)
            return NotFound($"Stock for ProductId {productId} not found.");
        
        return Ok(result);
    }
}
