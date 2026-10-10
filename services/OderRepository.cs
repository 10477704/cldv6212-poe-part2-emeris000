using Azure;
using Azure.Data.Tables;
using CoffeeNChill.Functions.Models;
using Microsoft.Extensions.Logging;

namespace CoffeeNChill.Functions.Services;

/// <summary>
/// Repository for reading from and writing to the "Orders" Azure Table.
/// </summary>
public class OrderRepository
{
    private const string TableName = "Orders";
    private readonly TableClient _ordersTable;
    private readonly ILogger<OrderRepository> _logger;

    public OrderRepository(TableServiceClient tableServiceClient,
                           ILogger<OrderRepository> logger)
    {
        _logger = logger;
        _ordersTable = tableServiceClient.GetTableClient(TableName);
        _ordersTable.CreateIfNotExists();
        _logger.LogInformation("OrderRepository initialised for table '{Table}'", TableName);
    }

    public async Task UpsertOrderAsync(Order order, CancellationToken ct = default)
    {
        await _ordersTable.UpsertEntityAsync(order, TableUpdateMode.Replace, ct);

        _logger.LogInformation(
            "Order {OrderId} saved to Orders table (Partition={Partition}, Status={Status})",
            order.RowKey, order.PartitionKey, order.Status);
    }

    public async Task<Order?> GetOrderAsync(string orderDate,
                                            string orderId,
                                            CancellationToken ct = default)
    {
        try
        {
            var response = await _ordersTable.GetEntityAsync<Order>(orderDate, orderId, cancellationToken: ct);
            return response.Value;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            _logger.LogWarning("Order {OrderId} on {OrderDate} was not found.", orderId, orderDate);
            return null;
        }
    }

    public async Task<List<Order>> GetOrdersByDateAsync(string orderDate,
                                                        CancellationToken ct = default)
    {
        var results = new List<Order>();

        await foreach (var order in _ordersTable.QueryAsync<Order>(
            o => o.PartitionKey == orderDate, cancellationToken: ct))
        {
            results.Add(order);
        }

        return results;
    }
}