using Azure;
using Azure.Data.Tables;

namespace CoffeeNChill.Functions.Models;

/// <summary>
/// Represents an order entity stored in the "Orders" Azure Table.
/// PartitionKey = OrderDate (yyyy-MM-dd) — groups all orders placed on the same day.
/// RowKey       = OrderId (e.g. "ORD-2026-8801").
/// </summary>
public class Order : ITableEntity
{
    public string PartitionKey { get; set; } = string.Empty;
    public string RowKey { get; set; } = string.Empty;
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }

    public string CustomerName { get; set; } = string.Empty;

    /// <summary>
    /// Comma-separated list of item SKUs (e.g. "COF-001,PAS-104").
    /// Azure Table Storage does not support List&lt;string&gt; natively.
    /// </summary>
    public string SelectedItemSKUs { get; set; } = string.Empty;

    public double TotalPrice { get; set; }

    public DateTime OrderTimestamp { get; set; }

    /// <summary>
    /// Barista lifecycle status. One of:
    /// "Received" -> "Preparing" -> "Ready" -> "Collected".
    /// </summary>
    public string Status { get; set; } = "Received";

    public DateTime? StatusUpdatedAt { get; set; }
}