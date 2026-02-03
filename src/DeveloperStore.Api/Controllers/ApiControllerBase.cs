using DeveloperStore.Application.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace DeveloperStore.Api.Controllers;

public abstract class ApiControllerBase : ControllerBase
{
    protected IActionResult FromResult<T>(Result<T> result)
    {
        if (result.Success)
        {
            return Ok(result.Data);
        }

        return result.ErrorType switch
        {
            "ResourceNotFound" => NotFound(new ErrorResponse(result.ErrorType, result.Error ?? string.Empty, result.Detail ?? string.Empty)),
            "ValidationError" => BadRequest(new ErrorResponse(result.ErrorType, result.Error ?? string.Empty, result.Detail ?? string.Empty)),
            _ => StatusCode(500, new ErrorResponse("InternalError", result.Error ?? "Unexpected error", result.Detail ?? string.Empty))
        };
    }
}
