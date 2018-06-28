using System;
using System.Threading;
using System.Threading.Tasks;
using IoT.Protocol.Soap;
using static IoT.Protocol.Upnp.UpnpServices;

namespace IoT.Protocol.Upnp.Services
{
    public sealed class ContentDirectoryService : ServiceContext
    {
        public ContentDirectoryService(SoapControlEndpoint endpoint, string deviceId) :
            base(endpoint, new Uri($"{deviceId}-MS/upnp.org-ContentDirectory-1/control", UriKind.Relative), ContentDirectory)
        {
        }

        public ContentDirectoryService(SoapControlEndpoint endpoint, Uri controlUri) : base(endpoint, controlUri, ContentDirectory)
        {
        }

        public ContentDirectoryService(SoapControlEndpoint endpoint) : base(endpoint, ContentDirectory)
        {
        }

        public async Task<string> BrowseAsync(string parent, string filter = null, uint index = 0, uint count = 50,
            string sortCriteria = null, CancellationToken cancellationToken = default)
        {
            return (await InvokeAsync("Browse", cancellationToken,
                    ("ObjectID", parent), ("BrowseFlag", "BrowseDirectChildren"),
                    ("Filter", filter ?? "*"), ("StartingIndex", index),
                    ("RequestedCount", count), ("SortCriteria", sortCriteria ?? "")).
                ConfigureAwait(false))["Result"];
        }
    }
}