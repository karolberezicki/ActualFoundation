using Foundation.Features.Ucp.Models;

namespace Foundation.Features.Ucp;

[ApiController]
[Route("checkout-sessions")]
public class UcpCheckoutController : ControllerBase
{
    private readonly IUcpCheckoutService _checkoutService;

    public UcpCheckoutController(IUcpCheckoutService checkoutService)
    {
        _checkoutService = checkoutService;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCheckoutSessionRequest request)
    {
        try
        {
            var response = await _checkoutService.CreateSessionAsync(request);
            return StatusCode(201, response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, BuildErrorResponse(ex.Message));
        }
    }

    [HttpGet("{sessionId}")]
    public async Task<IActionResult> Get(string sessionId)
    {
        try
        {
            var response = await _checkoutService.GetSessionAsync(sessionId);
            if (response == null)
            {
                return NotFound(BuildErrorResponse($"Session '{sessionId}' not found."));
            }

            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, BuildErrorResponse(ex.Message));
        }
    }

    [HttpPut("{sessionId}")]
    public async Task<IActionResult> Update(string sessionId, [FromBody] UpdateCheckoutSessionRequest request)
    {
        try
        {
            var response = await _checkoutService.UpdateSessionAsync(sessionId, request);
            if (response == null)
            {
                return NotFound(BuildErrorResponse($"Session '{sessionId}' not found."));
            }

            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, BuildErrorResponse(ex.Message));
        }
    }

    [HttpPost("{sessionId}/complete")]
    public async Task<IActionResult> Complete(string sessionId, [FromBody] CompleteCheckoutSessionRequest request)
    {
        try
        {
            var response = await _checkoutService.CompleteSessionAsync(sessionId, request);
            if (response == null)
            {
                return NotFound(BuildErrorResponse($"Session '{sessionId}' not found."));
            }

            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, BuildErrorResponse(ex.Message));
        }
    }

    [HttpPost("{sessionId}/cancel")]
    public async Task<IActionResult> Cancel(string sessionId)
    {
        try
        {
            var response = await _checkoutService.CancelSessionAsync(sessionId);
            if (response == null)
            {
                return NotFound(BuildErrorResponse($"Session '{sessionId}' not found."));
            }

            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, BuildErrorResponse(ex.Message));
        }
    }

    private static UcpCheckoutSessionResponse BuildErrorResponse(string message)
    {
        return new UcpCheckoutSessionResponse
        {
            Status = "ERROR",
            Messages =
            [
                new UcpMessage
                {
                    Type = "error",
                    Code = "INTERNAL_ERROR",
                    Severity = "ERROR",
                    Content = message,
                },
            ],
        };
    }
}