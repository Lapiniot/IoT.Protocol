using IoT.Protocol.Soap;

namespace IoT.Protocol.Upnp;

#pragma warning disable CA1000 // Do not declare static members on generic types

public interface IUpnpServiceFactory<out T> where T : SoapActionInvoker, IUpnpServiceFactory<T>
{
    static abstract T Create(SoapControlEndpoint endpoint, Uri controlUri);
}