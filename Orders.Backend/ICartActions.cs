using Orders.Shared;
using System.Threading.Tasks;

namespace Orders.Backend
{
    public interface ICartActions
    {
        Task AddAsync(Product product);

        void Delete();
    }
}