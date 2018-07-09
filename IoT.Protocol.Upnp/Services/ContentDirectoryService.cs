using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using IoT.Protocol.Soap;
using static IoT.Protocol.Upnp.UpnpServices;

namespace IoT.Protocol.Upnp.Services
{
    public sealed class ContentDirectoryService : SoapActionInvoker
    {
        public ContentDirectoryService(SoapControlEndpoint endpoint, Uri controlUri) : base(endpoint, controlUri, ContentDirectory)
        {
        }

        public ContentDirectoryService(SoapControlEndpoint endpoint) : base(endpoint, ContentDirectory)
        {
        }

        public Task<IDictionary<string, string>> BrowseAsync(string parent, string filter = null,
            string flags = null, string sortCriteria = null,
            uint index = 0, uint count = 50, CancellationToken cancellationToken = default)
        {
            return InvokeAsync("Browse", cancellationToken,
                ("ObjectID", parent), ("BrowseFlag", flags ?? "BrowseDirectChildren"),
                ("Filter", filter ?? "*"), ("StartingIndex", index),
                ("RequestedCount", count), ("SortCriteria", sortCriteria ?? ""));
        }
    }
}