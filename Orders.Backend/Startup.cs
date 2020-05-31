using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Orders.Backend.Data;
using System;

[assembly: FunctionsStartup(typeof(Orders.Backend.Startup))]


namespace Orders.Backend
{
    class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            string SqlConnection = Environment.GetEnvironmentVariable("ConnectionStrings:SqlConnectionString");

            Console.WriteLine($"Connstr is: {SqlConnection}");


            builder.Services.AddDbContext<OrdersContext>(
                options => options.UseSqlServer(SqlConnection));
        }
    }
}