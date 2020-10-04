using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Newtonsoft.Json;
using Orders.Shared;
using System;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Orders.Backend
{
    public static class CartFunctions
    {
        
        private static string GetUsername(this ClaimsPrincipal claimsPrincipal)
        {
#if DEBUG
            return "testUser";
#else
            return claimsPrincipal.FindFirst("name").Value;
#endif
        }
    }
}
