using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Newtonsoft.Json;
using Orders.Shared;
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
            var username = claimsPrincipal.GetUsername();

            var entityId = new EntityId("CartEntity", username);

            var state = await client.ReadEntityStateAsync<CartEntity>(entityId);

            return req.CreateResponse(state.EntityState?.Cart ?? new Cart());
        }

        [FunctionName(nameof(AddProduct))]
        public static async Task<HttpResponseMessage> AddProduct(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "mycart/products")] HttpRequestMessage req,
            [DurableClient] IDurableClient client, ClaimsPrincipal claimsPrincipal)
        {
            var username = claimsPrincipal.GetUsername();
            var entityId = new EntityId("CartEntity", username);
            
            var body = await req.Content.ReadAsStringAsync();

            var product = JsonConvert.DeserializeObject<Product>(body);

            var awaiter = client.GetTimestampAwaiter(entityId);

            await client.SignalEntityAsync<ICartActions>(entityId, x => x.SetOwner(username));

            await client.SignalEntityAsync<ICartActions>(entityId, x => x.Add(product));

            await awaiter.SignalsProcessed();

            return req.CreateResponse(HttpStatusCode.Created);
        }

        [FunctionName(nameof(DeleteCart))]
        public static async Task<HttpResponseMessage> DeleteCart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "mycart")] HttpRequestMessage req,
            [DurableClient] IDurableClient client, ClaimsPrincipal claimsPrincipal)
        {
            var username = claimsPrincipal.GetUsername();
            var entityId = new EntityId("CartEntity", username);

            var awaiter = client.GetDeletedAwaiter(entityId);

            await client.SignalEntityAsync<ICartActions>(entityId, x => x.Delete());

            await awaiter.SignalsProcessed();

            return req.CreateResponse(HttpStatusCode.Accepted);
        }

        [FunctionName(nameof(DispatchCart))]
        public static async Task<HttpResponseMessage> DispatchCart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "mycart/dispatch")] HttpRequestMessage req,
            [DurableClient] IDurableClient client, ClaimsPrincipal claimsPrincipal,
            [ServiceBus("ordersQueue", Connection = "ServiceBusConnection")] IAsyncCollector<Cart> collector)
        {
            var username = claimsPrincipal.GetUsername();
            var entityId = new EntityId("CartEntity", username);
            var state = await client.ReadEntityStateAsync<CartEntity>(entityId);

            if (!state.EntityExists)
            {
                // we can't call dispatch on a non existing entity
                return req.CreateErrorResponse(HttpStatusCode.BadRequest, "You can't call dispatch on a cart which doesn't exist");
            }

            await collector.AddAsync(state.EntityState.Cart);

            var awaiter = client.GetDeletedAwaiter(entityId);

            // empty cart once it has been dispatched
            await client.SignalEntityAsync<ICartActions>(entityId, x => x.Delete());

            await awaiter.SignalsProcessed();

            return req.CreateResponse(HttpStatusCode.Accepted);
        }

        private static string GetUsername(this ClaimsPrincipal claimsPrincipal)
        {
#if DEBUG
            return "testUser";
#else
            return claimsPrincipal.FindFirst("name").Value;
#endif
        }
    }
}
