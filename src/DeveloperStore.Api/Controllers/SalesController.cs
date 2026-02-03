using System.Linq;
using DeveloperStore.Application.Contracts;
using DeveloperStore.Application.Sales.Commands;
using DeveloperStore.Application.Sales.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DeveloperStore.Api.Controllers;

[ApiController]
[Route("sales")]
public sealed class SalesController : ApiControllerBase
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
            .Select(q => new KeyValuePair<string, string?>(q.Key, q.Value.ToString()))
            .ToDictionary(pair => pair.Key, pair => pair.Value);

        var result = await _mediator.Send(new GetSalesPagedQuery(page, size, order, filters));
        return FromResult(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetSaleByIdQuery(id));
        return FromResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateSaleCommand command)
    {
        var result = await _mediator.Send(command);
        return FromResult(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateSaleCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest(new ErrorResponse("ValidationError", "Invalid request", "Route id does not match body id."));
        }

        var result = await _mediator.Send(command);
        return FromResult(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _mediator.Send(new DeleteSaleCommand(id));
        return FromResult(result);
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id)
    {
        var result = await _mediator.Send(new CancelSaleCommand(id));
        return FromResult(result);
    }

    [HttpPost("{saleId:guid}/items/{itemId:guid}/cancel")]
    public async Task<IActionResult> CancelItem(Guid saleId, Guid itemId)
    {
        var result = await _mediator.Send(new CancelSaleItemCommand(saleId, itemId));
        return FromResult(result);
    }
}
