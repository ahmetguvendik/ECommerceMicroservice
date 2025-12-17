using Microsoft.AspNetCore.Mvc;

namespace DeliveryService.WebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok("Delivery Service OK");
}

