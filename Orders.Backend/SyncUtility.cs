using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using System;
using System.Threading.Tasks;

namespace Orders.Backend
{
    public static class SyncUtility
    {
        public interface IEntityAwaiter
        {
            Task SignalsProcessed();
        }

        private class DeletedAwaiter : IEntityAwaiter
        {
            private IDurableEntityClient _client;
            private EntityId _id;

            public DeletedAwaiter(IDurableEntityClient client, EntityId id)
            {
                _client = client;
                _id = id;
            }

            public async Task SignalsProcessed()
            {
                var state = await _client.ReadEntityStateAsync<CartEntity>(_id);

                while (state.EntityExists) 
                {
                    await Task.Delay(500);

                    state = await _client.ReadEntityStateAsync<CartEntity>(_id);
                }
            }
        }

        private class TimeStampAwaiter : IEntityAwaiter
        {
            private IDurableEntityClient _client;
            private EntityId _id;
            private DateTime _timestamp;

            public TimeStampAwaiter(IDurableEntityClient client, EntityId id)
            {
                _client = client;
                _id = id;
                _timestamp = DateTime.Now;
            }

            public async Task SignalsProcessed()
            {
                var state = await _client.ReadEntityStateAsync<CartEntity>(_id);

                while ((state.EntityState?.TimeStamp).GetValueOrDefault(DateTime.MinValue) < _timestamp)
                {
                    await Task.Delay(500);

                    state = await _client.ReadEntityStateAsync<CartEntity>(_id);
                }
            }
        }

        public static IEntityAwaiter GetDeletedAwaiter(this IDurableEntityClient client, EntityId id)
        {
            return new DeletedAwaiter(client, id);
        }

        public static IEntityAwaiter GetTimestampAwaiter(this IDurableEntityClient client, EntityId id)
        {
            return new TimeStampAwaiter(client, id);
        }
    }
}
