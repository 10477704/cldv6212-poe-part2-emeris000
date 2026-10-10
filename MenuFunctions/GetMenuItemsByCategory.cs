using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;

namespace CoffeeNChill.Functions
{
    public class GetMenuItemsByCategory
    {
        [Function("GetMenuItemsByCategory")]
        public static async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "menu/category/{category}")] HttpRequestData req,
            string category,
            FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("GetMenuItemsByCategory");
            logger.LogInformation($"Fetching menu items for category: {category}");

            // TODO: Retrieve items where Category == category
            var items = new[] { new { Id = 1, Name = "Latte", Category = category } }; // Placeholder

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(items);
            return response;
        }
    }
}