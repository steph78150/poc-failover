using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace poc_failover
{
    public class MessageBus : IMessageBus, IMessageBusPublisher
    {
        private readonly ISubject<IMessage> _subject = new Subject<IMessage>();

        public void Publish<T>(T msg) where T : IMessage
        {
            _subject.OnNext(msg);
        }

         public IObservable<T> GetMessageStream<T>() where T : IMessage => _subject.OfType<T>();
    }
}

