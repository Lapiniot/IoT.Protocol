using IoT.Protocol.Soap;

namespace IoT.Protocol.Upnp;

public interface IUpnpServiceFactory<T> where T : SoapActionInvoker, IUpnpServiceFactory<T>
{
    static abstract T Create(SoapControlEndpoint endpoint, Uri controlUri);
}