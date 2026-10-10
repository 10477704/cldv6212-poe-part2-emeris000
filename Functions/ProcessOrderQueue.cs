using System.Text;
using System.Text.Json;
using Azure.Storage.Queues.Models;
using CoffeeNChill.Functions.Models;
using CoffeeNChill.Functions.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CoffeeNChill.Functions.Functions;

/// <summary>
/// Queue-triggered Azure Function — ORDER CONSUMER.
/// Decodes Base64 payload produced by OrderQueueProducer.
/// </summary>
public class ProcessOrderQueue
{
    private const int StatusDelaySeconds = 5;

    private readonly ILogger<ProcessOrderQueue> _logger;

    public ProcessOrderQueue(ILogger<ProcessOrderQueue> logger)
    {
        _logger = logger;
    }

    [Function(nameof(ProcessOrderQueue))]
    public async Task Run(
        [QueueTrigger("order-processing-queue",
            Connection = "AzureWebJobsStorage")] QueueMessage queueMessage,
        FunctionContext context)
    {
        try
        {
            _logger.LogInformation("Queue message received from order-processing-queue.");

            var orderRepository = context.InstanceServices
                .GetRequiredService<OrderRepository>();

            // ⭐ Decode Base64 payload
            var raw = queueMessage.Body.ToString();
            string json;
            try
            {
                var bytes = Convert.FromBase64String(raw);
                json = Encoding.UTF8.GetString(bytes);
            }
            catch (FormatException)
            {
                // Fallback: if not Base64 (e.g. legacy messages), treat as raw JSON
                _logger.LogWarning("Queue message was not Base64 encoded. Treating as raw JSON.");
                json = raw;
            }

            OrderMessage? orderMessage;
            try
            {
                orderMessage = JsonSerializer.Deserialize<OrderMessage>(
                    json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Malformed JSON on order-processing-queue. Discarding.");
                return;
            }

            if (orderMessage is null ||
                string.IsNullOrWhiteSpace(orderMessage.OrderId) ||
                string.IsNullOrWhiteSpace(orderMessage.CustomerName) ||
                orderMessage.TotalPrice <= 0)
            {
                _logger.LogWarning("Queue message failed validation. Discarding.");
                return;
            }

            var orderDate = orderMessage.OrderTimestamp.ToString("yyyy-MM-dd");

            var order = new Order
            {
                PartitionKey = orderDate,
                RowKey = orderMessage.OrderId,
                CustomerName = orderMessage.CustomerName,
                SelectedItemSKUs = string.Join(",", orderMessage.SelectedItemSKUs),
                TotalPrice = orderMessage.TotalPrice,
                OrderTimestamp = orderMessage.OrderTimestamp,
                Status = "Received",
                StatusUpdatedAt = DateTime.UtcNow
            };

            await orderRepository.UpsertOrderAsync(order);
            _logger.LogInformation("Order {OrderId} persisted with Status=Received.", order.RowKey);

            string[] lifecycle = { "Preparing", "Ready", "Collected" };

            foreach (var status in lifecycle)
            {
                await Task.Delay(TimeSpan.FromSeconds(StatusDelaySeconds));

                order.Status = status;
                order.StatusUpdatedAt = DateTime.UtcNow;

                await orderRepository.UpsertOrderAsync(order);
                _logger.LogInformation("Order {OrderId} status advanced to {Status}.", order.RowKey, status);
            }

            _logger.LogInformation("Order {OrderId} fully processed (Collected).", order.RowKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "FATAL: ProcessOrderQueue failed with exception.");
            throw;
        }
    }
}