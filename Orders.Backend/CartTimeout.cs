using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Orders.Backend
{
    public static class CartTimeout
    {
        public static async Task ResetTimeoutAsync(this IDurableClient client, EntityId instanceId)
        {
            var status = await client.GetStatusAsync(instanceId.EntityKey);

            if (status != null && status.RuntimeStatus == OrchestrationRuntimeStatus.Running)
            {
                await client.RaiseEventAsync(instanceId.EntityKey, "TimeoutReset");
            }
            else
            {
                await client.StartNewAsync(nameof(StartTimeoutAsync), instanceId.EntityKey);
            }
        }

        public static async Task CancelTimeoutAsync(this IDurableClient client, EntityId instanceId)
        {
            var status = await client.GetStatusAsync(instanceId.EntityKey);

            if (status != null && status.RuntimeStatus == OrchestrationRuntimeStatus.Running)
            {
                await client.TerminateAsync(instanceId.EntityKey, "Entity deleted");
            }
        }

        [FunctionName(nameof(StartTimeoutAsync))]
        public static async Task StartTimeoutAsync([OrchestrationTrigger] IDurableOrchestrationContext ctx)
        {
            var entityId = ctx.InstanceId;

            using (var timeout = new CancellationTokenSource())
            {
                DateTime dueTime = ctx.CurrentUtcDateTime.AddMinutes(1);
                Task durableTimeout = ctx.CreateTimer(dueTime, timeout.Token);

                Task cancelEvent = ctx.WaitForExternalEvent("TimeoutReset");
                if (cancelEvent == await Task.WhenAny(cancelEvent, durableTimeout))
                {
                    timeout.Cancel();
                    ctx.ContinueAsNew(entityId);
                }
                else
                {
                    var proxy = ctx.CreateEntityProxy<ICartActions>(entityId);

                    proxy.Delete();
                }
            }

        }
    }
}
