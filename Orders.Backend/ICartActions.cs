using Orders.Shared;
using System.Threading.Tasks;

namespace Orders.Backend
{
    public interface ICartActions
    {
        void Add(Product product);

        void SetOwner(string owner);

        Task<Cart> Get();

        void Delete();
    }
}