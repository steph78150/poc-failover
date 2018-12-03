using System;

namespace poc_failover
{
    public interface IMessageBus
    {
        void Send<T>(T msg) 
            where T: IMessage;
        IObservable<IMessage> GetMessageStream();
    }
    
}