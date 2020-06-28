using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace Orders.UI.Authentication
{
    public class OfflineAccountClaimsPrincipalFactory : AccountClaimsPrincipalFactory<RemoteUserAccount>
    {
        private readonly IServiceProvider _services;

        public OfflineAccountClaimsPrincipalFactory(IServiceProvider services, IAccessTokenProviderAccessor accessor) : base(accessor)
        {
            _services = services;
        }

        public override async ValueTask<ClaimsPrincipal> CreateUserAsync(RemoteUserAccount account, RemoteAuthenticationUserOptions options)
        {
            Console.WriteLine("CreateUser");
            //var localVehiclesStore = _services.GetRequiredService<LocalVehiclesStore>();

            var result = await base.CreateUserAsync(account, options);
            //if (result.Identity.IsAuthenticated)
            //{
            //    await localVehiclesStore.SaveUserAccountAsync(result);
            //}
            //else
            //{
            //    result = await localVehiclesStore.LoadUserAccountAsync();
            //}

            return result;
        }
    }
}
