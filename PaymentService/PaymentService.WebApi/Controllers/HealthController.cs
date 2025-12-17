using Microsoft.AspNetCore.Mvc;

namespace PaymentService.WebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok("Payment Service OK");
}

