using System;

namespace IoT.Protocol
{
    public interface IConnectedEndpoint<in TRequest, TResponse> :
        IControlEndpoint<TRequest, TResponse>,
        IDisposable
    {
        void Connect();
        void Close();
    }
}