using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Orders.Shared;
using System;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Orders.Backend
{
    public static class CartFunctions
    {
        [FunctionName(nameof(GetCart))]
        public static async Task<HttpResponseMessage> GetCart(
           [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "mycart")] HttpRequestMessage req,
           [DurableClient] IDurableEntityClient client, ClaimsPrincipal claimsPrincipal)
        {
            var username = claimsPrincipal.FindFirst("name").Value;

            var entityId = new EntityId("CartEntity", username);

            var state = await client.ReadEntityStateAsync<CartEntity>(entityId);

            return req.CreateResponse(state.EntityState?.Cart ?? new Cart());
        }

        [FunctionName(nameof(AddProduct))]
        public static async Task<HttpResponseMessage> AddProduct(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "mycart/products")] HttpRequestMessage req,
            [DurableClient] IDurableClient client, ClaimsPrincipal claimsPrincipal, ILogger log)
        {
            var username = claimsPrincipal.FindFirst("name").Value;
            var entityId = new EntityId("CartEntity", username);

            var body = await req.Content.ReadAsStringAsync();
            var product = JsonConvert.DeserializeObject<Product>(body);

            var tuple = new Tuple<EntityId, Product>(entityId, product);

            var instanceId = await client.StartNewAsync("AddProductOrchestration", tuple);

            DurableOrchestrationStatus status;

            while ((status = await client.GetStatusAsync(instanceId)).RuntimeStatus != OrchestrationRuntimeStatus.Completed)
            {
                log.LogInformation("waiting 100ms");
                await Task.Delay(100);
            }

            return req.CreateResponse(HttpStatusCode.Created, status.Output);
        }

        [FunctionName(nameof(AddProductOrchestration))]
        public static async Task<Cart> AddProductOrchestration([OrchestrationTrigger] IDurableOrchestrationContext ctx)
        {
            var (entityId, product) = ctx.GetInput<Tuple<EntityId, Product>>();

            var proxy = ctx.CreateEntityProxy<ICartActions>(entityId);

            proxy.Add(product);

            var currentState = await proxy.Get();

            return currentState;
        }

        [FunctionName(nameof(DeleteCart))]
        public static async Task<HttpResponseMessage> DeleteCart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "mycart")] HttpRequestMessage req,
            [DurableClient] IDurableEntityClient client, ClaimsPrincipal claimsPrincipal)
        {
            var username = claimsPrincipal.FindFirst("name").Value;
            var entityId = new EntityId("CartEntity", username);

            await client.SignalEntityAsync<ICartActions>(entityId, x => x.Delete());

            return req.CreateResponse(HttpStatusCode.Accepted);
        }

        [FunctionName(nameof(DispatchCart))]
        public static async Task<HttpResponseMessage> DispatchCart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "mycart/dispatch")] HttpRequestMessage req,
            [DurableClient] IDurableEntityClient client, ClaimsPrincipal claimsPrincipal,
            [ServiceBus("ordersQueue", Connection = "ServiceBusConnection")] IAsyncCollector<Cart> collector)
        {
            var username = claimsPrincipal.FindFirst("name").Value;
            var entityId = new EntityId("CartEntity", username);
            var state = await client.ReadEntityStateAsync<CartEntity>(entityId);

            if (!state.EntityExists)
            {
                // we can't call dispatch on a non existing entity
                return req.CreateErrorResponse(HttpStatusCode.BadRequest, "You can't call dispatch on a cart which doesn't exist");
            }

            await collector.AddAsync(state.EntityState.Cart);

            // empty cart once it has been dispatched
            await client.SignalEntityAsync<ICartActions>(entityId, x => x.Delete());

            return req.CreateResponse(HttpStatusCode.Accepted);
        }
    }
}
