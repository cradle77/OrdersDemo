using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Orders.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orders.Backend
{
    public class CartEntity : ICartActions
    {
        private IAsyncCollector<SignalRMessage> signalRMessages;

        public CartEntity(IAsyncCollector<SignalRMessage> signalRMessages)
        {
            this.signalRMessages = signalRMessages;
        }

        public Cart Cart { get; set; } = new Cart();

        public DateTime TimeStamp { get; set; }

        public async void Add(Product product)
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

            this.TimeStamp = DateTime.Now;

            await this.signalRMessages.AddAsync(new SignalRMessage() { UserId = this.Cart.Owner, Target = "CartChanged", Arguments = new object[0] });
        }

        public Task<Cart> Get()
        {
            return Task.FromResult(this.Cart);
        }

        public void SetOwner(string owner)
        {
            this.Cart.Owner = owner;
        }

        public async void Delete()
        {
            Entity.Current.DeleteState();

            await this.signalRMessages.AddAsync(new SignalRMessage() { UserId = this.Cart.Owner, Target = "CartChanged", Arguments = new object[0] });
        }

        [FunctionName(nameof(CartEntity))]
        public static Task Run([EntityTrigger] IDurableEntityContext ctx,
            [SignalR(HubName = "CartNotifications",
            ConnectionStringSetting = "AzureSignalRConnectionString")] IAsyncCollector<SignalRMessage> signalRMessages)
        {
            if (!ctx.HasState)
            {
                ctx.SetState(new CartEntity(signalRMessages));
            }

            return ctx.DispatchAsync<CartEntity>(signalRMessages);
        }
    }
}
