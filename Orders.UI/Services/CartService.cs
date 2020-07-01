using Orders.Shared;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Blazored.LocalStorage;

namespace Orders.UI.Services
{
    public class CartService
    {
        private readonly HttpClient _client;
        private readonly ILocalStorageService _localStorageService;
        private readonly NetworkService _networkService;
        private Cart _cart;

        private const string Key = "Cart";

        public event EventHandler CartUpdated;

        public CartService(
            HttpClient client,
            ILocalStorageService localStorageService,
            NetworkService networkService)
        {
            _client = client;
            _localStorageService = localStorageService;
            _networkService = networkService;

            _networkService.OnlineChanged += OnlineChanged;
        }

        public bool PendingSynchronization { get; private set; }

        public async Task AddProductToCartAsync(Product product)
        {
            if (_cart == null) return;

            var item = _cart.Items.SingleOrDefault(p => p.Product.Id == product.Id);
            if (item != null)
            {
                item.Quantity += 1;
            }
            else
            {
                _cart.Items.Add(new CartItem()
                {
                    Product = product,
                    Quantity = 1
                });
            }

            await SaveCartAsync();
            await SynchronizeAsync();

            OnCartUpdated();
        }

        public async Task EmptyCartAsync()
        {
            _cart = new Cart();

            await SaveCartAsync();
            await SynchronizeAsync();

            OnCartUpdated();
        }

        public async Task DispatchCartAsync()
        {
            if (!_networkService.IsOnline)
            {
                throw new InvalidOperationException("Cannot dispatch cart because app is offline");
            }
            await _client.PostAsJsonAsync($"api/mycart/dispatch", new object());

            this.CartUpdated?.Invoke(this, EventArgs.Empty);
        }

        public async Task<Cart> GetCartAsync()
        {
            if (_networkService.IsOnline)
            {
                try
                {
                    _cart = await _client.GetFromJsonAsync<Cart>($"api/mycart/");
                    await SaveCartAsync();
                }
                catch
                {
                    // race condition for when the access token becomes available
                }
            }
            else
            {
                _cart = await _localStorageService.GetItemAsync<Cart>(Key);
            }

            return _cart ??= new Cart();
        }

        private async Task SaveCartAsync()
        {
            await _localStorageService.SetItemAsync(Key, _cart);
        }

        private async Task SynchronizeAsync()
        {
            if (_cart == null || !_networkService.IsOnline)
            {
                PendingSynchronization = true;
                return;
            }
            if (_cart.Items.Count == 0)
            {
                await _client.DeleteAsync($"api/mycart/");
            }
            else
            {
                await _client.PutAsJsonAsync($"api/mycart", _cart.Items);
            }

            await _localStorageService.RemoveItemAsync(Key);
            _cart = null;
            PendingSynchronization = false;
        }

        private void OnCartUpdated()
        {
            CartUpdated?.Invoke(this, EventArgs.Empty);
        }

        private async void OnlineChanged(object sender, EventArgs e)
        {
            await SynchronizeAsync();

            OnCartUpdated();
        }

    }
}
