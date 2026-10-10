using CoffeeNChill.Functions.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace CoffeeNChill.Functions.Functions;

/// <summary>
/// HTTP query functions for retrieving orders from the "Orders" Azure Table.
/// </summary>
public class OrderQueryFunctions
{
    private readonly OrderRepository _orderRepository;
    private readonly ILogger<OrderQueryFunctions> _logger;

    public OrderQueryFunctions(OrderRepository orderRepository,
                               ILogger<OrderQueryFunctions> logger)
    {
        _orderRepository = orderRepository;
        _logger = logger;
    }

    /// <summary>
    /// GET /api/orders/by-id/{orderDate}/{orderId}
    /// ⚠️ Changed from "orders/{orderDate}/{orderId}" to avoid ambiguity
    /// with "orders/by-date/{orderDate}".
    /// </summary>
    [Function("GetOrderById")]
    public async Task<IActionResult> GetOrderById(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get",
            Route = "orders/by-id/{orderDate}/{orderId}")] HttpRequest req,
        string orderDate,
        string orderId,
        CancellationToken ct)
    {
        _logger.LogInformation("GET /api/orders/by-id/{OrderDate}/{OrderId}", orderDate, orderId);

        var order = await _orderRepository.GetOrderAsync(orderDate, orderId, ct);

        if (order is null)
        {
            return new NotFoundObjectResult(new
            {
                error = $"Order '{orderId}' on {orderDate} was not found."
            });
        }

        return new OkObjectResult(order);
    }

    /// <summary>
    /// GET /api/orders/by-date/{orderDate}
    /// </summary>
    [Function("GetOrdersByDate")]
    public async Task<IActionResult> GetOrdersByDate(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get",
            Route = "orders/by-date/{orderDate}")] HttpRequest req,
        string orderDate,
        CancellationToken ct)
    {
        _logger.LogInformation("GET /api/orders/by-date/{OrderDate}", orderDate);

        var orders = await _orderRepository.GetOrdersByDateAsync(orderDate, ct);
        return new OkObjectResult(orders);
    }
}