using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication.Internal;
using Orders.UI.Services;

namespace Orders.UI.Authentication
{
    public class OfflineAccountClaimsPrincipalFactory : AccountClaimsPrincipalFactory<RemoteUserAccount>
    {
        private readonly NetworkService _networkService;
        private readonly UserService _userService;

        public OfflineAccountClaimsPrincipalFactory(
            IAccessTokenProviderAccessor accessor,
            NetworkService networkService,
            UserService userService) : base(accessor)
        {
            _networkService = networkService;
            _userService = userService;
        }

        public override async ValueTask<ClaimsPrincipal> CreateUserAsync(RemoteUserAccount account, RemoteAuthenticationUserOptions options)
        {
            ClaimsPrincipal result = await base.CreateUserAsync(account, options);
            if (result.Identity.IsAuthenticated)
            {
                await _userService.SetAsync(result);
            }
            else if (!_networkService.IsOnline)
            {
                return await _userService.GetAsync();
            }

            return result;
        }

      
    }
}
