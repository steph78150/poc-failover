using System;

namespace poc_failover
{
    public interface INode : IDisposable
    {
        void Start();

        void Stop();

        string RealServerName {get;}

        string CurrentIdentity {get;}

    }
}
