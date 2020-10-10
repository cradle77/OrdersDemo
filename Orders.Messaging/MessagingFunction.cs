using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.IO;
using System.Threading.Tasks;

namespace Orders.Messaging
{
    public static class MessagingFunction
    {
        [FunctionName("CartChanged")]
        public static async Task<IActionResult> SendMessage(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            [SignalR(HubName = "CartNotifications", 
            ConnectionStringSetting = "AzureSignalRConnectionString")] IAsyncCollector<SignalRMessage> signalRMessages,
            ILogger log)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            string message = data?.message;
            string[] arguments = data?.arguments?.ToObject<string[]>() ?? new string[0];
            string userId = data?.userId;

            if (!string.IsNullOrWhiteSpace(message))
            {
                await signalRMessages.AddAsync(new SignalRMessage()
                {
                    Target = message,
                    Arguments = arguments,
                    UserId = userId
                });
            }

            return message != null
                ? (ActionResult)new OkResult()
                : new BadRequestObjectResult("message is mandatory in the body");
        }

        [FunctionName("negotiate")]
        public static IActionResult Negotiate(
            [HttpTrigger(AuthorizationLevel.Anonymous)] HttpRequest req,
            [SignalRConnectionInfo
                // security is not wired up yet
                (HubName = "CartNotifications", UserId = "{headers.x-ms-client-principal-id}",
                ConnectionStringSetting = "AzureSignalRConnectionString")]
                SignalRConnectionInfo connectionInfo, ILogger log)
        {
            req.Headers.TryGetValue("x-ms-client-principal-id", out var userId);

            log.LogInformation($"New user connected: {userId}");

            // connectionInfo contains an access key token with a 
            // name identifier claim set to the authenticated user
            return new OkObjectResult(connectionInfo);
        }
    }
}
