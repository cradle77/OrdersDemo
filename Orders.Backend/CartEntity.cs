using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Newtonsoft.Json;
using Orders.Shared;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Orders.Backend
{
    public class CartEntity : ICartActions
    {
        public Cart Cart { get; set; } = new Cart();

        public Task AddAsync(Product product)
        {
            var item = this.Cart.Items.SingleOrDefault(p => p.Product.Id == product.Id);

            if (item != null)
            {
                item.Quantity += 1;
            }
            else
            {
                this.Cart.Items.Add(new CartItem() 
                {
                    Product = product,
                    Quantity = 1
                });
            }

            return Task.CompletedTask;
        }

        public void Delete()
        {
            Entity.Current.DeleteState();
        }

        [FunctionName(nameof(CartEntity))]
        public static Task Run([EntityTrigger] IDurableEntityContext ctx)
        {
            if (!ctx.HasState)
            {
                ctx.SetState(new CartEntity());
            }

            return ctx.DispatchAsync<CartEntity>();
        }


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
            [DurableClient] IDurableEntityClient client, ClaimsPrincipal claimsPrincipal)
        {
            var username = claimsPrincipal.FindFirst("name").Value;
            var entityId = new EntityId("CartEntity", username);

            var body = await req.Content.ReadAsStringAsync();

            var product = JsonConvert.DeserializeObject<Product>(body);

            await client.SignalEntityAsync<ICartActions>(entityId, x => x.AddAsync(product));

            return req.CreateResponse(HttpStatusCode.Created);
        }

        [FunctionName(nameof(DeleteCart))]
        public static async Task<HttpResponseMessage> DeleteCart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "mycart")] HttpRequestMessage req,
            [DurableClient] IDurableEntityClient client, ClaimsPrincipal claimsPrincipal)
        {
            var username = claimsPrincipal.FindFirst("name").Value;
            var entityId = new EntityId("CartEntity", username);

            var body = await req.Content.ReadAsStringAsync();

            var product = JsonConvert.DeserializeObject<Product>(body);

            await client.SignalEntityAsync<ICartActions>(entityId, x => x.Delete());

            return req.CreateResponse(HttpStatusCode.Accepted);
        }
    }
}
