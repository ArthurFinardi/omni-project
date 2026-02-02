using DeveloperStore.Application.Contracts;
using DeveloperStore.Application.Sales.Commands;
using DeveloperStore.Application.Sales.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DeveloperStore.Api.Controllers;

[ApiController]
[Route("sales")]
public sealed class SalesController : ControllerBase
{
    private readonly IMediator _mediator;

    public SalesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery(Name = "_page")] int page = 1,
        [FromQuery(Name = "_size")] int size = 10,
        [FromQuery(Name = "_order")] string? order = null)
    {
        var filters = Request.Query
            .Where(q => !q.Key.StartsWith("_"))
            .ToDictionary(q => q.Key, q => q.Value.ToString());

        var result = await _mediator.Send(new GetSalesPagedQuery(page, size, order, filters));
        return HandleResult(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetSaleByIdQuery(id));
        return HandleResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateSaleCommand command)
    {
        var result = await _mediator.Send(command);
        return HandleResult(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateSaleCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest(new ErrorResponse("ValidationError", "Invalid request", "Route id does not match body id."));
        }

        var result = await _mediator.Send(command);
        return HandleResult(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _mediator.Send(new DeleteSaleCommand(id));
        return HandleResult(result);
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id)
    {
        var result = await _mediator.Send(new CancelSaleCommand(id));
        return HandleResult(result);
    }

    [HttpPost("{saleId:guid}/items/{itemId:guid}/cancel")]
    public async Task<IActionResult> CancelItem(Guid saleId, Guid itemId)
    {
        var result = await _mediator.Send(new CancelSaleItemCommand(saleId, itemId));
        return HandleResult(result);
    }

    private IActionResult HandleResult<T>(Result<T> result)
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
