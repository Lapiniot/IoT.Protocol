using IoT.Protocol.Soap;

namespace IoT.Protocol.Upnp;

public interface IUpnpServiceFactory<out T> where T : SoapActionInvoker, IUpnpServiceFactory<T>
{
#pragma warning disable CA1000 // Do not declare static members on generic types
    static abstract T Create(SoapControlEndpoint endpoint, Uri controlUri);
#pragma warning restore CA1000 // Do not declare static members on generic types
}