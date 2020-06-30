using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using Orders.Shared;

namespace Orders.UI.Services
{
    public class ProductService
    {
        private readonly HttpClient _httpClient;
        private readonly NetworkService _networkService;
        private readonly ILocalStorageService _localStorageService;

        public ProductService(HttpClient httpClient, NetworkService networkService, ILocalStorageService localStorageService)
        {
            _httpClient = httpClient;
            _networkService = networkService;
            _localStorageService = localStorageService;
        }

        public async Task<Product[]> GetProductsAsync()
        {
            string key = $"{GetType().Name}.{nameof(GetProductsAsync)}";
            Product[] result;
            if (_networkService.IsOnline)
            {
                result = await _httpClient.GetFromJsonAsync<Product[]>("api/products");

                await _localStorageService.SetItemAsync(key, result);
            }
            else
            {
                result = await _localStorageService.GetItemAsync<Product[]>(key);
            }

            return result;
        }
    }
}
