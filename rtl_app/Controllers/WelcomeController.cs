using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace rtl_app.Controllers;
[ApiController]
public class WelcomeController : ControllerBase
{
    public WelcomeController()
    {
    }


    [HttpGet("/", Name = "welcome")]
    public IActionResult Welcome()
    {
        return Ok("Application running");
    }

}