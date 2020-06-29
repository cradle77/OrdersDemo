using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Newtonsoft.Json;
using Orders.Shared;
using System;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
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
            var username = "des";// claimsPrincipal.FindFirst("name").Value;

            var entityId = new EntityId("CartEntity", username);

            var state = await client.ReadEntityStateAsync<CartEntity>(entityId);

            return req.CreateResponse(state.EntityState?.Cart ?? new Cart());
        }

        [FunctionName(nameof(AddProduct))]
        public static async Task<HttpResponseMessage> AddProduct(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "mycart/products")] HttpRequestMessage req,
            [DurableClient] IDurableClient client, ClaimsPrincipal claimsPrincipal)
        {
            var username = "des";// claimsPrincipal.FindFirst("name").Value;
            var entityId = new EntityId("CartEntity", username);
            
            var body = await req.Content.ReadAsStringAsync();

            var product = JsonConvert.DeserializeObject<Product>(body);

            var awaiter = client.GetTimestampAwaiter(entityId);

            await client.SignalEntityAsync<ICartActions>(entityId, x => x.SetOwner(username));

            await client.SignalEntityAsync<ICartActions>(entityId, x => x.Add(product));

            await client.ResetTimeoutAsync(entityId);

            await awaiter.SignalsProcessed();

            return req.CreateResponse(HttpStatusCode.Created);
        }

        [FunctionName(nameof(SetCartItems))]
        public static async Task<HttpResponseMessage> SetCartItems(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "mycart")] HttpRequestMessage req,
            [DurableClient] IDurableClient client, ClaimsPrincipal claimsPrincipal)
        {
            var username = "des";// claimsPrincipal.FindFirst("name").Value;
            var entityId = new EntityId("CartEntity", username);

            var body = await req.Content.ReadAsStringAsync();

            var cartItems = JsonConvert.DeserializeObject<CartItem[]>(body);

            var awaiter = client.GetTimestampAwaiter(entityId);

            await client.SignalEntityAsync<ICartActions>(entityId, x => x.SetOwner(username));

            await client.SignalEntityAsync<ICartActions>(entityId, x => x.Set(cartItems));

            await client.ResetTimeoutAsync(entityId);

            await awaiter.SignalsProcessed();

            return req.CreateResponse(HttpStatusCode.Accepted);
        }

        [FunctionName(nameof(DeleteCart))]
        public static async Task<HttpResponseMessage> DeleteCart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "mycart")] HttpRequestMessage req,
            [DurableClient] IDurableClient client, ClaimsPrincipal claimsPrincipal)
        {
            var username = "des";// claimsPrincipal.FindFirst("name").Value;
            var entityId = new EntityId("CartEntity", username);

            var awaiter = client.GetDeletedAwaiter(entityId);

            await client.SignalEntityAsync<ICartActions>(entityId, x => x.Delete());
            await client.CancelTimeoutAsync(entityId);

            await awaiter.SignalsProcessed();

            return req.CreateResponse(HttpStatusCode.Accepted);
        }

        [FunctionName(nameof(DispatchCart))]
        public static async Task<HttpResponseMessage> DispatchCart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "mycart/dispatch")] HttpRequestMessage req,
            [DurableClient] IDurableClient client, ClaimsPrincipal claimsPrincipal,
            [ServiceBus("ordersQueue", Connection = "ServiceBusConnection")] IAsyncCollector<Cart> collector)
        {
            var username = "des";// claimsPrincipal.FindFirst("name").Value;
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
            await client.CancelTimeoutAsync(entityId);

            await awaiter.SignalsProcessed();

            return req.CreateResponse(HttpStatusCode.Accepted);
        }
    }
}
