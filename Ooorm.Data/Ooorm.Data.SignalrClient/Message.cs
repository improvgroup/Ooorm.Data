using System;
using System.Collections.Generic;
using System.Threading;

namespace Ooorm.Data.SignalrClient
{
    public class Message<T>
    {
        public bool OriginIsServer { get; set; } = false;
        public Guid EndpointId { get; set; }
        public Guid MessageId { get; set; } = Guid.NewGuid();
        public string Type { get; set; } = typeof(T).Name;
        public T Payload { get; set; }
    }

    internal class MessageRecord<T>
    {
        public Message<T> Message { get; set; }
        public ManualResetEvent Recieved { get; set; }

        public void Wait() => Recieved.WaitOne();
    }

    internal class RequestCollection<T>
    {
        public MessageRecord<T> Next(Message<T> payload)
        {
            var record = new MessageRecord<T>
            {
                Message = payload,
                Recieved = new ManualResetEvent(false),
            };
            this[payload.MessageId] = record;
            return record;
        }

        private readonly Dictionary<Guid, MessageRecord<T>> requests = new Dictionary<Guid, MessageRecord<T>>();

        public MessageRecord<T> this[Guid key]
        {
            get => requests[key];
            private set => requests[key] = value;
        }

        public void Remove(MessageRecord<T> record) => requests.Remove(record.Message.MessageId);
    }
}
