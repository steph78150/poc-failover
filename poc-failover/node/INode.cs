using System;

namespace poc_failover
{
    public interface INode : IDisposable
    {
        void Start();

        void Stop();

        string ServerName {get;}

        string CurrentIdentity {get;}

    }
}
