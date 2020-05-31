using System;
using System.Collections.Generic;
using System.Text;

namespace Orders.Shared
{
    public class CartItem
    {
        public Product Product { get; set; }
        public int Quantity { get; set; }
        public decimal Amount 
        {
            get { return (this.Product?.Cost).GetValueOrDefault() * this.Quantity; }
        }
    }
}
