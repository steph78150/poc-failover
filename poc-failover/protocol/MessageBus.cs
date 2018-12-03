using System;
using System.Reactive.Subjects;

namespace poc_failover
{
    public class MessageBus : IMessageBus
    {
        private readonly ISubject<IMessage> _subject = new Subject<IMessage>();


        public void Send<T>(T msg) where T : IMessage
        {
            _subject.OnNext(msg);
        }

         public IObservable<IMessage> GetMessageStream() => _subject;
    }
}

