using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ooorm.Data.Core.AsyncExtensions
{
    public static class AsyncExtensions
    {
        public static async IAsyncEnumerable<V> Map<U,V>(this IAsyncEnumerable<U> source, Func<U, V> map)
        {
            await foreach (var item in source)
                yield return map(item);
        }

        public static async IAsyncEnumerable<V> Map<U,V>(this IAsyncEnumerable<U> source, Func<U, Task<V>> map)
        {
            await foreach (var item in source)
                yield return await map(item);
        }

        public static async IAsyncEnumerable<U> Filter<U>(this IAsyncEnumerable<U> source, Func<U, bool> predicate)
        {
            await foreach (var item in source)
                if (predicate(item))
                    yield return item;
        }

        public static async IAsyncEnumerable<U> Filter<U>(this IAsyncEnumerable<U> source, Func<U, Task<bool>> predicate)
        {
            await foreach (var item in source)
                if (await predicate(item))
                    yield return item;
        }

        public static async Task<P> Reduce<U,P>(this IAsyncEnumerable<U> source, Func<P, U, P> project, P initial = default)
        {
            var value = initial;
            await foreach (var item in source)
                value = project(value, item);
            return value;
        }

        public static async Task<P> Reduce<U, P>(this IAsyncEnumerable<U> source, Func<P, U, Task<P>> project, P initial = default)
        {
            var value = initial;
            await foreach (var item in source)
                value = await project(value, item);
            return value;
        }        

        public static async IAsyncEnumerable<IAsyncEnumerable<V>> Branch<U, V>(
            this IAsyncEnumerable<U> source,
            params Func<U, V>[] branches)
            where U : V
        {
            IAsyncEnumerable<V> branch(U value)
            {
                foreach (var map in branches)
                    yield return map(value);
            }

            await foreach (var item in source)
                yield return branch(item);
        }

        public static async IAsyncEnumerable<IAsyncEnumerable<V>> Branch<U, V>(
            this IAsyncEnumerable<U> source,
            params Func<U, Task<V>>[] branches)
            where U : V
        {
            async IAsyncEnumerable<V> branch(U value)
            {
                foreach (var map in branches)
                    yield return await map(value);
            }

            await foreach (var item in source)                        
                yield return branch(item);
        }

        public static async IAsyncEnumerable<IAsyncEnumerable<V>> Branch<U, V>(
            this IAsyncEnumerable<U> source, 
            params (Func<U, bool> predicate, Func<U, V> map)[] branches) 
            where U : V
        {
            IAsyncEnumerable<V> branch(U value)
            {
                foreach (var (predicate, map) in branches)
                    if (predicate(value))
                        yield return map(value);
            }

            await foreach (var item in source)
                yield return branch(item);
        }

        public static async IAsyncEnumerable<IAsyncEnumerable<V>> Branch<U, V>(
            this IAsyncEnumerable<U> source,
            params (Func<U, bool> predicate, Func<U, Task<V>> map)[] branches)
            where U : V
        {
            async IAsyncEnumerable<V> branch(U value)
            {
                foreach (var (predicate, map) in branches)
                    if (predicate(value))
                        yield return await map(value);
            }

            await foreach (var item in source)
                yield return branch(item);
        }

        public static async IAsyncEnumerable<V> Match<U, V>(
            this IAsyncEnumerable<U> source,
            params (Func<U, bool> predicate, Func<U, V> map)[] branches)
            where U : V
        {
            await foreach (var item in source)
                foreach (var (predicate, map) in branches)
                    if (predicate(item))
                    {
                        yield return map(item);
                        break;
                    }
        }

        public static async IAsyncEnumerable<V> Match<U, V>(
            this IAsyncEnumerable<U> source,
            params (Func<U, bool> predicate, Func<U, Task<V>> map)[] branches)
            where U : V
        {
            await foreach (var item in source)
                foreach (var (predicate, map) in branches)
                    if (predicate(item))
                    {
                        yield return await map(item);
                        break;
                    }
        }
    }
}
