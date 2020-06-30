using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Blazored.LocalStorage;

namespace Orders.UI.Services
{
    public class UserService
    {
        private readonly ILocalStorageService _localStorageService;

        private const string Key = "User";

        public UserService(ILocalStorageService localStorageService)
        {
            _localStorageService = localStorageService;
        }

        public async Task<ClaimsPrincipal> GetAsync()
        {
            var claimDatas = await _localStorageService.GetItemAsync<ClaimData[]>(Key);
            return claimDatas != null
                ? new ClaimsPrincipal(new ClaimsIdentity(claimDatas.Select(c => new Claim(c.Type, c.Value)), "appAuth"))
                : new ClaimsPrincipal(new ClaimsIdentity());
        }

        public Task ResetAsync()
        {
            return _localStorageService.RemoveItemAsync(Key);
        }

        public async Task SetAsync(ClaimsPrincipal principal)
        {
            var claimDatas = principal.Claims
                .Select(c => new ClaimData { Type = c.Type, Value = c.Value })
                .ToArray();
            await _localStorageService.SetItemAsync(Key, claimDatas);
        }

        class ClaimData
        {
            public string Type { get; set; }
            public string Value { get; set; }
        }
    }
}
