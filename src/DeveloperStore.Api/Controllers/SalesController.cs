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
    private static readonly HashSet<string> AllowedOrderFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "saleNumber",
        "saleDate",
        "totalAmount"
    };

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
        if (page < 1)
            return BadRequest(new ErrorResponse("ValidationError", "Invalid pagination", "_page must be greater than or equal to 1."));

        if (size < 1 || size > 100)
            return BadRequest(new ErrorResponse("ValidationError", "Invalid pagination", "_size must be between 1 and 100."));

        if (!IsValidOrder(order, out var orderValidationError))
            return BadRequest(new ErrorResponse("ValidationError", "Invalid ordering", orderValidationError));

        // Keep business filters including "_min*" and "_max*" (docs), only exclude paging/sorting parameters.
        var filters = Request.Query
            .Where(q =>
                !q.Key.Equals("_page", StringComparison.OrdinalIgnoreCase) &&
                !q.Key.Equals("_size", StringComparison.OrdinalIgnoreCase) &&
                !q.Key.Equals("_order", StringComparison.OrdinalIgnoreCase))
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
            return BadRequest(new ErrorResponse("ValidationError", "Invalid request", "Route id does not match body id."));

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

    private static bool IsValidOrder(string? order, out string detail)
    {
        detail = string.Empty;
        if (string.IsNullOrWhiteSpace(order))
            return true;

        // Docs show quotes in examples. Accept optional surrounding quotes.
        order = order.Trim().Trim('"');

        foreach (var segment in order.Split(',', StringSplitOptions.RemoveEmptyEntries))
        {
            var parts = segment.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
                continue;

            var field = parts[0];
            if (!AllowedOrderFields.Contains(field))
            {
                detail = $"Ordering field '{field}' is not allowed. Allowed fields: {string.Join(", ", AllowedOrderFields)}.";
                return false;
            }

            if (parts.Length > 1)
            {
                var direction = parts[1];
                if (!direction.Equals("asc", StringComparison.OrdinalIgnoreCase) &&
                    !direction.Equals("desc", StringComparison.OrdinalIgnoreCase))
                {
                    detail = $"Ordering direction '{direction}' is invalid. Use 'asc' or 'desc'.";
                    return false;
                }
            }

            if (parts.Length > 2)
            {
                detail = $"Ordering segment '{segment.Trim()}' is invalid.";
                return false;
            }
        }

        return true;
    }
}
