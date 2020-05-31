﻿using Microsoft.AspNetCore.Components.Authorization;
using Orders.Shared;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Orders.UI.Services
{
    public class CartService
    {
        private HttpClient _client;
        private AuthenticationStateProvider _stateProvider;

        public event EventHandler CartUpdated;

        public CartService(HttpClient client, AuthenticationStateProvider authenticationStateProvider)
        {
            _client = client;
            _stateProvider = authenticationStateProvider;
        }

        public async Task AddProductToCartAsync(Product item)
        {
            var username = await this.GetUsernameAsync();

            await _client.PostAsJsonAsync($"api/cart/{username}/products", item);

            this.CartUpdated?.Invoke(this, EventArgs.Empty);
        }

        public async Task EmptyCartAsync()
        {
            var username = await this.GetUsernameAsync();

            await _client.DeleteAsync($"api/cart/{username}");

            this.CartUpdated?.Invoke(this, EventArgs.Empty);
        }

        public async Task DispatchCartAsync()
        {
            var username = await this.GetUsernameAsync();

            await _client.PostAsJsonAsync($"api/cart/{username}/dispatch", new object());

            this.CartUpdated?.Invoke(this, EventArgs.Empty);
        }

        public async Task<Cart> GetCart()
        {
            try
            {
                var username = await this.GetUsernameAsync();

                return await _client.GetFromJsonAsync<Cart>($"api/cart/{username}");
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
    }
}
