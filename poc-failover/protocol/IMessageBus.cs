using System;

namespace poc_failover
{
    public interface IMessageBusPublisher
    {
         void Publish<T>(T msg)  where T: IMessage;
    }

    public interface IMessageBus : IMessageBusPublisher
    {
        IObservable<T> GetMessageStream<T>() where T: IMessage;
    }
    
}