namespace Foundation.Features.UltimateFind;

[ApiController]
[Route("api/[controller]")]
public class UltimateFindController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok("Ok");
    }
}