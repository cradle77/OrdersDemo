using Orders.Shared;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Orders.Backend
{
    public interface ICartActions
    {
        void Add(Product product);

        void Set(IEnumerable<CartItem> cartItems);

        void SetOwner(string owner);

        Task<Cart> Get();

        void Delete();
    }
}