using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Ooorm.Data.Core.AsyncExtensions
{
    public class AsyncEnumerationException : Exception
    {
        public AsyncEnumerationException(string message) : base(message)
        {
        }
    }

    public static class EnumerationAsyncExtensions
    {
        public static async Task<T> UnwrapSingle<T>(this IAsyncEnumerable<T> source, string message = null, [CallerMemberName] string caller = null)
        {
            message = $"{message ?? ""} [in {caller}]";

            int count = 0;
            T result = default;
            await foreach (var item in source)
            {
                if (count == 0)
                    result = item;
                else
                    throw new AsyncEnumerationException($"Expected one but found many: {message}");
            }
            if (count == 0)
                throw new AsyncEnumerationException($"Expected one but found none: {message}");

            return result;
        }

        public static async Task<T> UnwrapFirst<T>(this IAsyncEnumerable<T> source, string message = null, [CallerMemberName] string caller = null)
        {
            message = $"{message ?? ""} [in {caller}]";

            int count = 0;
            T result = default;
            await foreach (var item in source)
            {                
                result = item;
                source.Complete();
                else
                    throw new AsyncEnumerationException($"Expected one but found many: {message}");
            }
            if (count == 0)
                throw new AsyncEnumerationException($"Expected one but found none: {message}");

            return result;
        }


    }
}
