using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.IO;
using System.Threading.Tasks;

namespace Orders.Backend
{
    public class MessagingFunctions
    {
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
