using Kelson.Common.Async;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Ooorm.Data.Volatile
{
    public class VolatileRepository<T> : ICrudRepository<T> where T : IDbItem<TId> where TId : struct, IEquatable<TId>
    {
        protected class Bucket
        {
            public readonly int IdRangeStart;
            public readonly int IdRangeEnd;
            public readonly Actor<SortedList<int, T>> Data = new Actor<SortedList<int, T>>(new SortedList<int, T>());

            public Bucket(int start, int end) => (IdRangeStart, IdRangeEnd) = (start, end);
        }

        protected volatile int currentId = 1;

        protected static readonly int BUCKET_SIZE = 100;

        protected readonly Func<IDatabase> database;

        protected readonly object bucketMutationLock = new object();
        protected readonly Dictionary<int, Bucket> Buckets = new Dictionary<int, Bucket>();

        protected Bucket GetBucket(int id)
            => Buckets.ContainsKey(id / BUCKET_SIZE) ? Buckets[id / BUCKET_SIZE] : null;

        protected bool TryGetBucket(int id, out Bucket bucket)
            => (bucket = GetBucket(id)) != null;

        protected Bucket GetOrAddBucket(int id)
        {
            if (TryGetBucket(id, out Bucket bucket))
                return bucket;
            else
            {
                int start = id / BUCKET_SIZE;
                bucket = new Bucket(start, start + BUCKET_SIZE - 1);
                lock (bucketMutationLock)
                {
                    Buckets[start] = bucket;
                }
                return bucket;
            }            
        }

        protected Dictionary<Bucket, List<int>> GetBuckets(IEnumerable<int> ids)
        {
            var buckets = new Dictionary<Bucket, List<int>>();
            foreach (var id in ids)
                if (TryGetBucket(id, out Bucket bucket))
                    if (buckets.ContainsKey(bucket))
                        buckets[bucket].Add(id);
                    else
                        buckets.Add(bucket, new List<int> { id });
                else
                    throw new KeyNotFoundException($"Id [{id}] does not exist");
            return buckets;
        }

        protected Dictionary<Bucket, List<T>> GetBuckets(IEnumerable<T> items)
        {
            var buckets = new Dictionary<Bucket, List<T>>();
            foreach (var item in items)
                if (item.ID != default && TryGetBucket(item.ID, out Bucket bucket))
                    if (buckets.ContainsKey(bucket))
                        buckets[bucket].Add(item);
                    else
                        buckets.Add(bucket, new List<T> { item });
                else
                    throw new KeyNotFoundException($"Item with ID [{item.ID}] does not exist");
            return buckets;
        }

        protected readonly bool manageIds;

        public VolatileRepository(Func<IDatabase> db, bool manageIds = true) => (database, this.manageIds) = (db, manageIds);

        public Task<int> CreateTable() => Task.FromResult(Buckets.Count > 0 ? 0 : 1);

        public Task<int> DropTable()
        {
            int count = Buckets.Count > 0 ? 1 : 0;
            Buckets.Clear();
            return Task.FromResult(count);
        }

        public async Task<int> Delete(params T[] values) => await Delete(values.Select(v => v.ID));

        private async Task<int> Delete(IEnumerable<int> ids)
        {
            int count = 0;
            await Task.Run(async () =>
            {
                foreach (var kvp in GetBuckets(ids))
                    await kvp.Key.Data.Do(d =>
                    {
                        foreach (var i in kvp.Value)
                        {
                            count++;
                            d.Remove(i);
                        }
                    });
            });
            return count;
        }

        public async Task<int> Delete(Expression<Func<T, bool>> predicate)
        {
            return await Task.Run(async () => {
                var test = predicate.Compile();
                int count = 0;
                foreach (var bucket in Buckets.Values)
                    await bucket.Data.Do(values =>
                    {
                        var remove = new List<int>();
                        foreach (var item in values)
                            if (test(item.Value))
                                remove.Add(item.Key);
                        foreach (var id in remove)
                            values.Remove(id);
                        count += remove.Count;
                    });
                return count;
            });
        }

        public async Task<int> Delete<TParam>(Expression<Func<T, TParam, bool>> predicate, TParam param)
        {
            return await Task.Run(async () => {
                var test = predicate.Compile();
                int count = 0;
                foreach (var bucket in Buckets.Values)
                    await bucket.Data.Do(values =>
                    {
                        var remove = new List<int>();
                        foreach (var item in values)
                            if (test(item.Value, param))
                                remove.Add(item.Key);
                        foreach (var id in remove)
                            values.Remove(id);
                        count += remove.Count;
                    });
                return count;
            });
        }

        public async Task<IEnumerable<T>> Read()
        {
            List<T> results = new List<T>();
            foreach (var bucket in Buckets.Values)
                await bucket.Data.Do(values => results.AddRange(values.Values));
            return results;
        }

        public async Task<IEnumerable<object>> ReadUntyped() => (await Read()).Select(i => (object)i);

        public async Task<T> Read(int id)
        {
            T result = default;
            if (TryGetBucket(id, out Bucket bucket))
                await bucket.Data.Do(values => result = values[id]);
            return result;
        }

        public async Task<IEnumerable<T>> Read(Expression<Func<T, bool>> predicate)
            => (await Read()).Where(predicate.Compile());

        public async Task<IEnumerable<T>> Read<TParam>(Expression<Func<T, TParam, bool>> predicate, TParam param)
        {
            var test = predicate.Compile();
            return (await Read()).Where(r => test(r, param));
        }

        public async Task<int> Update(params T[] items)
        {
            return await Task.Run(async () =>
            {
                int count = 0;
                foreach (var kvp in GetBuckets(items))
                    await kvp.Key.Data.Do(values =>
                    {
                        foreach (var item in kvp.Value)
                            values[item.ID] = item;
                    });
                return count;
            });
        }

        public async Task<int> Write(params T[] items)
        {
            if (items.Length == 0)
                return 0;
            if (manageIds)
                items[0].ID = currentId++;
            else if (items[0].ID == default)
                throw new InvalidOperationException("When volatile repo does not manage ids all items must have ids set before writing");
            var bucket = GetOrAddBucket(items[0].ID);
            foreach (var item in items)
            {
                if (manageIds)
                    item.ID = item.ID == items[0].ID ? items[0].ID : currentId++;
                else if (item.ID == default)
                    throw new InvalidOperationException("When volatile repo does not manage ids all items must have ids set before writing");
                if (item.ID > bucket.IdRangeEnd)
                    bucket = GetOrAddBucket(item.ID);
                await bucket.Data.Do(values => values[item.ID] = item);
            }
            return items.Length;
        }
    }
}
