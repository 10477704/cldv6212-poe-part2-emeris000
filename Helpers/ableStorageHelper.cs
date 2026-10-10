using Azure.Data.Tables;
using System;

namespace CoffeeNChill.Functions.Helpers
{
    public static class TableStorageHelper
    {
        private const string TABLE_NAME = "MenuItems";
        private static TableClient? _tableClient;
        private static readonly object _lock = new object();

        public static TableClient GetTableClient()
        {
            if (_tableClient != null)
                return _tableClient;

            lock (_lock)
            {
                if (_tableClient != null)
                    return _tableClient;

                var connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage")
                                       ?? "UseDevelopmentStorage=true";

                _tableClient = new TableClient(connectionString, TABLE_NAME);
                _tableClient.CreateIfNotExists();

                return _tableClient;
            }
        }
    }
}