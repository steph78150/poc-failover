using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;

namespace poc_failover
{
    public class MessageBus : IMessageBus, IMessageBusPublisher
    {


        private readonly ISubject<IMessage> _subject = new Subject<IMessage>();
        private readonly NetworkRandomizer _randomizer;

        public MessageBus(NetworkRandomizer randomizer) {
            _randomizer = randomizer;
        }

        public void Publish<T>(T msg) where T : IMessage
        {
            Thread.Sleep(_randomizer.GetRandomDelay());
            _subject.OnNext(msg);
        }

         public IObservable<T> GetMessageStream<T>() where T : IMessage => _subject.OfType<T>();
    }
}

