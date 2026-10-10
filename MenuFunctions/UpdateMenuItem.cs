using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;

namespace CoffeeNChill.Functions
{
    public class UpdateMenuItem
    {
        [Function("UpdateMenuItem")]
        public static async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "menu/{category}/{id}")] HttpRequestData req,
            string category,
            string id,
            FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("UpdateMenuItem");
            logger.LogInformation($"Updating menu item: {category}/{id}");

            // Read the body to get update data
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            // TODO: Update logic in the database based on ID and Category

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(new { message = "Item updated", category = category, id = id, data = requestBody });
            return response;
        }
    }
}