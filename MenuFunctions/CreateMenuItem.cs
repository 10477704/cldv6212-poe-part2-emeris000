using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;

namespace CoffeeNChill.Functions
{
    public class CreateMenuItem
    {
        [Function("CreateMenuItem")]
        public static async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "menu")] HttpRequestData req,
            FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("CreateMenuItem");
            logger.LogInformation("Creating a new menu item.");

            // Read body
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            // TODO: Deserialize requestBody and save to database

            var response = req.CreateResponse(HttpStatusCode.Created);
            await response.WriteAsJsonAsync(new { message = "Item created successfully", data = requestBody });
            return response;
        }
    }
}