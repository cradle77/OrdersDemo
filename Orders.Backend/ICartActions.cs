using Orders.Shared;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Orders.Backend
{
    public interface ICartActions
    {
        void Add(Product product);

        Task<Cart> Get();

        void Delete();
    }
}