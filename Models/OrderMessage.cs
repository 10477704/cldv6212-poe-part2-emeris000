namespace CoffeeNChill.Functions.Models;

/// <summary>
/// DTO representing the JSON payload pushed onto the "order-processing-queue".
/// Mirrors the shape in the PoE spec.
/// </summary>
public class OrderMessage
{
    public string OrderId { get; set; } = string.Empty;

    public string CustomerName { get; set; } = string.Empty;

    public List<string> SelectedItemSKUs { get; set; } = new();

    public double TotalPrice { get; set; }

    public DateTime OrderTimestamp { get; set; }
}