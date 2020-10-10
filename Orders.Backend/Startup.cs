using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Orders.Backend.Data;
using System;

[assembly: WebJobsStartup(typeof(Orders.Backend.Startup))]


namespace Orders.Backend
{
    class Startup : IWebJobsStartup
    {
        public void Configure(IWebJobsBuilder builder)
        {
            string SqlConnection = Environment.GetEnvironmentVariable("ConnectionStrings:SqlConnectionString") ??
                Environment.GetEnvironmentVariable("SQLAZURECONNSTR_SqlConnectionString");

            builder.Services.AddDbContext<OrdersContext>(
                options => options.UseSqlServer(SqlConnection, configure => 
                {
                    configure.EnableRetryOnFailure();
                }));
        }
    }
}