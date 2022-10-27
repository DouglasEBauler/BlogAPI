using BlogAPI.Attributes;
using Microsoft.AspNetCore.Mvc;

// Health Check
namespace BlogAPI.Controllers;

[ApiController]
[Route("")]
public class HomeController : ControllerBase
{
    [HttpGet("")]
    public IActionResult Get(
        [FromServices] IConfiguration config) 
    {
        return Ok(new { Enviroment = config.GetValue<string>("Env") });
    }
}
