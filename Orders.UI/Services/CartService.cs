using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.SignalR.Client;
using Orders.Shared;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Orders.UI.Services
{
    public class CartService : IDisposable
    {
        private HttpClient _client;
        private AuthenticationStateProvider _stateProvider;
        private HubConnection _hubConnection;

        public event EventHandler CartUpdated;

        public CartService(HttpClient client, AuthenticationStateProvider authenticationStateProvider)
        {
            _client = client;
            _stateProvider = authenticationStateProvider;

            _hubConnection = new HubConnectionBuilder()
                .WithUrl("http://127.0.0.1:7071/api/", options =>
                {
                    // note: test only, do not use in production
                    options.Headers.Add("x-ms-client-principal-id", "testUser");
                })
                .Build();

            _hubConnection.On("CartChanged", () =>
            {
                this.CartUpdated?.Invoke(this, EventArgs.Empty);
            });

            _hubConnection.StartAsync();
        }

        public async Task AddProductToCartAsync(Product item)
        {
            await _client.PostAsJsonAsync($"api/mycart/products", item);

            this.CartUpdated?.Invoke(this, EventArgs.Empty);
        }

        public async Task EmptyCartAsync()
        {
            await _client.DeleteAsync($"api/mycart/");

            this.CartUpdated?.Invoke(this, EventArgs.Empty);
        }

        public async Task DispatchCartAsync()
        {
            await _client.PostAsJsonAsync($"api/mycart/dispatch", new object());

            this.CartUpdated?.Invoke(this, EventArgs.Empty);
        }

        public async Task<Cart> GetCart()
        {
            try
            {
                return await _client.GetFromJsonAsync<Cart>($"api/mycart/");
            }
            catch
            {
                // race condition for when the access token becomes available
                return new Cart();
            }
            
        }

        private async Task<string> GetUsernameAsync()
        {
            var context = await _stateProvider.GetAuthenticationStateAsync();

            if (!context.User.Identity.IsAuthenticated)
            {
                throw new InvalidOperationException("User must be authenticated");
            }

            var username = context.User.Identity.Name;

            return username;
        }

        public void Dispose()
        {
            _hubConnection.DisposeAsync();
        }
    }
}
