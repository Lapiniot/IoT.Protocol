using System;
using System.Threading;
using System.Threading.Tasks;
using IoT.Protocol.Soap;

namespace IoT.Protocol.Upnp.Services
{
    public sealed class ContentDirectoryService : SoapActionInvoker
    {
        public ContentDirectoryService(SoapControlEndpoint endpoint, string deviceId) :
            base(endpoint, new Uri($"{deviceId}-MS/upnp.org-ContentDirectory-1/control", UriKind.Relative), UpnpServices.ContentDirectory)
        {
        }

        public async Task<string> BrowseAsync(string parent, string filter = null, uint index = 0, uint count = 50,
            string sortCriteria = null, CancellationToken cancellationToken = default)
        {
            return (await InvokeAsync("Browse", cancellationToken,
                ("ObjectID", parent), ("BrowseFlag", "BrowseDirectChildren"),
                ("Filter", filter ?? "*"), ("StartingIndex", index),
                ("RequestedCount", count), ("SortCriteria", sortCriteria ?? "")).ConfigureAwait(false))["Result"];
        }
    }
}