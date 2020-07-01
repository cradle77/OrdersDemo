using System.Collections.Generic;
using System.Linq;

namespace Orders.Shared
{
    public class Cart
    {
        public string Owner { get; set; }

        public List<CartItem> Items { get; set; } = new List<CartItem>();

        public decimal TotalAmount
        {
            get
            {
                return this.Items.Sum(x => x.Amount);
            }
        }

        public void Add(Product product)
        {
            var item = this.Items.SingleOrDefault(p => p.Product.Id == product.Id);

            if (item != null)
            {
                item.Quantity += 1;
            }
            else
            {
                this.Items.Add(new CartItem()
                {
                    Product = product,
                    Quantity = 1
                });
            }
        }
    }
}
