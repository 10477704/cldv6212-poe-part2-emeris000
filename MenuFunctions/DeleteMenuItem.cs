using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;

namespace CoffeeNChill.Functions
{
    public class DeleteMenuItem
    {
        [Function("DeleteMenuItem")]
        public static async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "menu/{id}")] HttpRequestData req,
            string id,
            FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("DeleteMenuItem");
            logger.LogInformation($"Deleting menu item with ID: {id}");

            // TODO: Delete logic based on ID

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteStringAsync($"Item {id} deleted successfully");
            return response;
        }
    }
}