using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Ooorm.Data.Core
{
    public class QueuedAsyncEnumerable<T> : IAsyncEnumerable<T>
    {        
        private readonly Func<QueuedAsyncEnumerator<T>> source;

        public QueuedAsyncEnumerable(Func<QueuedAsyncEnumerator<T>> source) => this.source = source;

        public IAsyncEnumerator<T> GetAsyncEnumerator() => source();
    }


    public class QueuedAsyncEnumerator<T> : IAsyncEnumerator<T>
    {
        private readonly AutoResetEvent advance = new AutoResetEvent(false);

        private bool _isComplete = false;
        public bool IsComplete => _isComplete || disposed;

        private T _current;
        public T Current => _current; 
        
        private readonly ConcurrentQueue<T> queue
            = new ConcurrentQueue<T>();

        internal void Complete()
        {
            _isComplete = true;
            advance.Set();
        }

        internal void Push(T item)
        {
            if (_isComplete)
                return;

            queue.Enqueue(item);
            advance.Set();
        }

        public QueuedAsyncEnumerator() { }

        private bool disposed = false;

        public async ValueTask DisposeAsync()
        {
            _isComplete = true;
            disposed = true;
        }

        public async ValueTask<bool> MoveNextAsync()
        {
            return await Task.Factory.StartNew(() =>
            {
                Get:

                if (IsComplete)
                    return false;                                

                if (queue.TryDequeue(out T item))
                {
                    _current = item;
                    return true;
                }
                else
                {
                    advance.WaitOne();
                    goto Get;
                }
            });
        }
    }
}
