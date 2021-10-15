using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Orders.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orders.Backend
{
    public class CartEntity
    {
        

        [FunctionName(nameof(CartEntity))]
        public static Task Run([EntityTrigger] IDurableEntityContext ctx)
        {
            if (!ctx.HasState)
            {
                //ctx.SetState(new CartEntity(ctx.EntityKey));
            }

            return ctx.DispatchAsync<CartEntity>();
        }
    }
}
