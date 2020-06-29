using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Orders.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orders.Backend
{
    public class CartEntity : ICartActions
    {
        public Cart Cart { get; set; } = new Cart();

        public DateTime TimeStamp { get; set; }

        public void Add(Product product)
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
        }

        public void Set(IEnumerable<CartItem> cartItems)
        {
            this.Cart.Items.Clear();
            this.Cart.Items.AddRange(cartItems);

            this.TimeStamp = DateTime.Now;
        }

        public Task<Cart> Get()
        {
            return Task.FromResult(this.Cart);
        }

        public void SetOwner(string owner)
        {
            this.Cart.Owner = owner;
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
    }
}
