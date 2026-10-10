using Azure;
using Azure.Data.Tables;

namespace CoffeeNChill.Functions.Models
{
    public class MenuItem : ITableEntity
    {
        // PartitionKey = Category (e.g., "Hot Drinks", "Cold Drinks")
        public string PartitionKey { get; set; } = string.Empty;

        // RowKey = Unique SKU (e.g., "COF-001", "PAS-104")
        public string RowKey { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public double Price { get; set; }
        public bool IsAvailable { get; set; } = true;

        // Required by ITableEntity
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
    }
}