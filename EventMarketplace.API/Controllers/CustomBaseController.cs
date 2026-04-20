using EventMarketplace.API.Responses;
using Microsoft.AspNetCore.Mvc;

namespace EventMarketplace.API.Controllers;

[ApiController]
public class CustomBaseController : ControllerBase
{
    public IActionResult ActionResultInstance<T>(CustomResponse<T> response)
    {
        return new ObjectResult(response)
        {
            StatusCode = response.StatusCode
        };
    }
}
