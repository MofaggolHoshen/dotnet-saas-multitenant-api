using Application.Common.Models;
using Domain.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public abstract class ApiController(ISender sender) : ControllerBase
{
    protected ISender Sender { get; } = sender;

    protected IActionResult HandleResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
        {
            return Ok(ApiResponse<T>.Ok(result.Value));
        }

        return result.Error.Code switch
        {
            "NotFound" => NotFound(ApiResponse<T>.Fail(result.Error.Message)),
            "Conflict" => Conflict(ApiResponse<T>.Fail(result.Error.Message)),
            "Validation" => BadRequest(ApiResponse<T>.Fail(result.Error.Message)),
            _ => StatusCode(500, ApiResponse<T>.Fail(result.Error.Message))
        };
    }

    protected IActionResult HandleResult(Result result)
    {
        if (result.IsSuccess)
        {
            return NoContent();
        }

        return result.Error.Code switch
        {
            "NotFound" => NotFound(ApiResponse<object>.Fail(result.Error.Message)),
            "Conflict" => Conflict(ApiResponse<object>.Fail(result.Error.Message)),
            "Validation" => BadRequest(ApiResponse<object>.Fail(result.Error.Message)),
            _ => StatusCode(500, ApiResponse<object>.Fail(result.Error.Message))
        };
    }
}
