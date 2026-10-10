using System.Text;
using System.Text.Json;
using Azure.Storage.Queues;
using CoffeeNChill.Functions.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace CoffeeNChill.Functions.Functions;

/// <summary>
/// HTTP-triggered function that acts as the ORDER PRODUCER.
/// Endpoint: POST /api/orders/queue
/// NOTE: Encodes the payload as Base64 for reliable queue-trigger binding.
/// </summary>
public class OrderQueueProducer
{
    private const string QueueName = "order-processing-queue";

    private readonly QueueClient _queueClient;
    private readonly ILogger<OrderQueueProducer> _logger;

    public OrderQueueProducer(QueueServiceClient queueServiceClient,
                              ILogger<OrderQueueProducer> logger)
    {
        _logger = logger;
        _queueClient = queueServiceClient.GetQueueClient(QueueName);
        _queueClient.CreateIfNotExists();
    }

    [Function("OrderQueueProducer")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "orders/queue")]
        HttpRequest req,
        CancellationToken ct)
    {
        _logger.LogInformation("POST /api/orders/queue received.");

        OrderMessage? order;
        try
        {
            order = await JsonSerializer.DeserializeAsync<OrderMessage>(
                req.Body,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true },
                ct);
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Malformed JSON in request body.");
            return new BadRequestObjectResult(new { error = "Invalid JSON payload." });
        }

        if (order is null ||
            string.IsNullOrWhiteSpace(order.OrderId) ||
            string.IsNullOrWhiteSpace(order.CustomerName) ||
            order.SelectedItemSKUs is null || order.SelectedItemSKUs.Count == 0 ||
            order.TotalPrice <= 0 ||
            order.OrderTimestamp == default)
        {
            _logger.LogWarning("Order failed validation.");
            return new BadRequestObjectResult(new
            {
                error = "Missing or invalid fields. Required: OrderId, CustomerName, " +
                        "SelectedItemSKUs (non-empty), TotalPrice (> 0), OrderTimestamp."
            });
        }

        // ⭐ Serialize AND Base64-encode for the isolated worker queue trigger
        var payload = JsonSerializer.Serialize(order);
        var base64Payload = Convert.ToBase64String(Encoding.UTF8.GetBytes(payload));

        try
        {
            await _queueClient.SendMessageAsync(base64Payload, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to enqueue order {OrderId}", order.OrderId);
            return new ObjectResult(new { error = "Queue connection failed." })
            {
                StatusCode = StatusCodes.Status503ServiceUnavailable
            };
        }

        _logger.LogInformation("Order {OrderId} queued successfully (base64).", order.OrderId);

        return new AcceptedResult(
            location: $"/api/orders/{order.OrderTimestamp:yyyy-MM-dd}/{order.OrderId}",
            value: new
            {
                message = "Order accepted for processing.",
                orderId = order.OrderId,
                queuedAt = DateTime.UtcNow
            });
    }
}