using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Orders.Shared;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Orders.Backend
{
    public static class WeatherForecastService
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        [FunctionName("WeatherForecast")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "weather")] HttpRequest req,
            ILogger log, ClaimsPrincipal claimsPrincipal)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            var username = claimsPrincipal.FindFirst("name").Value;

            var rng = new Random();
            var result =  Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)],
                Username = username
            })
            .ToArray();

            return new OkObjectResult(result);
        }
    }
}
