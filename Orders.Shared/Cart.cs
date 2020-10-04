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
    }
}
