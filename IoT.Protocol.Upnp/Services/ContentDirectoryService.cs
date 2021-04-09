using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using IoT.Protocol.Soap;
using static System.Globalization.CultureInfo;
using static IoT.Protocol.Upnp.UpnpServices;

namespace IoT.Protocol.Upnp.Services
{
    public enum BrowseMode
    {
        BrowseDirectChildren,
        BrowseMetadata
    }

    [ServiceSchema(ContentDirectory)]
    [CLSCompliant(false)]
    public sealed class ContentDirectoryService : SoapActionInvoker
    {
        public ContentDirectoryService(SoapControlEndpoint endpoint, Uri controlUri) :
            base(endpoint, controlUri, ContentDirectory)
        { }

        public ContentDirectoryService(SoapControlEndpoint endpoint) :
            base(endpoint, ContentDirectory)
        { }

        public Task<IReadOnlyDictionary<string, string>> BrowseAsync(string parent, string filter = null,
            BrowseMode mode = default, string sortCriteria = null,
            uint index = 0, uint count = 50, CancellationToken cancellationToken = default)
        {
            return InvokeAsync("Browse", new Dictionary<string, string>() {
                { "ObjectID", parent },
                { "BrowseFlag", mode.ToString() },
                { "Filter", filter ?? "*" },
                { "StartingIndex", index.ToString(InvariantCulture) },
                { "RequestedCount", count.ToString(InvariantCulture) },
                { "SortCriteria", sortCriteria ?? "" } },
                cancellationToken);
        }

        public Task<IReadOnlyDictionary<string, string>> SearchAsync(string container, string query, string filter = null,
            string sortCriteria = null, uint index = 0, uint count = 50, CancellationToken cancellationToken = default)
        {
            return InvokeAsync("Search", new Dictionary<string, string>() {
                { "ContainerID", container },
                { "SearchCriteria", query },
                { "Filter", filter ?? "*" },
                { "StartingIndex", index.ToString(InvariantCulture) },
                { "RequestedCount", count.ToString(InvariantCulture) },
                { "SortCriteria", sortCriteria ?? "" } },
                cancellationToken);
        }
    }
}