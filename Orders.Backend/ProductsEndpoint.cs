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

        public ProductsEndpoint(OrdersContext db)
        {
            _db = db;
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
