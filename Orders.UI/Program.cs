using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Orders.UI.Services;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Orders.UI
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("app");

            builder.Services.AddHttpClient("api", client =>
            {
                client.BaseAddress = new Uri("https://ordersdemo.azurewebsites.net");
            })
            .AddHttpMessageHandler(sp =>
             {
                 var handler = sp.GetService<AuthorizationMessageHandler>()
                     .ConfigureHandler(
                         authorizedUrls: new[] { "https://ordersdemo.azurewebsites.net" },
                         scopes: new[] { "https://BlazorB2C.onmicrosoft.com/49a5ea34-2ea6-42bb-9ed4-6076e169b1fc/Api.Access" });

                 return handler;
             });

            builder.Services.AddTransient(sp => sp.GetService<IHttpClientFactory>().CreateClient("api"));

            builder.Services.AddMsalAuthentication(options =>
            {
                builder.Configuration.Bind("AzureAdB2C", options.ProviderOptions.Authentication);
                
            });

            builder.Services.AddScoped<CartService>();

            await builder.Build().RunAsync();
        }
    }
}
