using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Orders.Backend.Data;
using System.Runtime.InteropServices.ComTypes;
using Microsoft.EntityFrameworkCore;

namespace Orders.Backend
{
    public class ProductsEndpoint
    {
        private OrdersContext _db;

        public ProductsEndpoint()
        {
            string SqlConnection = Environment.GetEnvironmentVariable("ConnectionStrings:SqlConnectionString") ??
                Environment.GetEnvironmentVariable("SQLAZURECONNSTR_SqlConnectionString");

            var builder = new DbContextOptionsBuilder<OrdersContext>()
                .UseSqlServer(SqlConnection, configure =>
                {
                    configure.EnableRetryOnFailure();
                });

            _db = new OrdersContext(builder.Options);
        }

        [FunctionName("Products")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "products")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Received call to fetch all the products.");

            var result = await _db.Products.ToListAsync();

            return new OkObjectResult(result);
        }
    }
}
