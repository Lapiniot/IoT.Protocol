using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using IoT.Protocol.Upnp.DIDL;
using IoT.Protocol.Upnp.Services;

namespace IoT.Protocol.Upnp
{
    public static class UpnpUtils
    {
        public static async Task<string> GetUpdateIdAsync(ContentDirectoryService service, string itemId, CancellationToken cancellationToken)
        {
            return (await service.BrowseAsync(itemId, mode: BrowseMode.BrowseMetadata, filter: "id", cancellationToken: cancellationToken).ConfigureAwait(false))["UpdateID"];
        }

        public static async Task<int[]> GetItemIndices(ContentDirectoryService service, string parentId, IEnumerable<string> ids, CancellationToken cancellationToken)
        {
            var data = await service.BrowseAsync(parentId, count: uint.MaxValue, cancellationToken: cancellationToken).ConfigureAwait(false);
            var playlists = DIDLXmlParser.Parse(data["Result"], false, false);
            var map = playlists.Select((p, index) => (p.Id, index)).ToDictionary(p => p.Id, p => p.index + 1);
            return ids.Select(id => map[id]).ToArray();
        }

        public static async Task WriteItemsMetadataTree(ContentDirectoryService service, IEnumerable<string> itemIds, XmlWriter writer, int maxDepth, CancellationToken cancellationToken)
        {
            var containerIds = new Stack<(string Id, int Depth)>();

            // Phase 1: get items metadata and sort out items and containers, items are copied immidiatelly to the output xml writer,
            // container ids are scheduled for future expansion by pushing to the containerIds stack structure
            foreach(var item in itemIds)
            {
                var data = await service.BrowseAsync(item, mode: BrowseMode.BrowseMetadata, cancellationToken: cancellationToken).ConfigureAwait(false);
                DIDLUtils.CopyItems(data["Result"], writer, containerIds, 0);
            }

            // Phase 2: iteratively pop container id from the stack and browse for its direct children sorting items and child containers again
            while(containerIds.TryPop(out var result))
            {
                if(result.Depth >= maxDepth) continue;
                var data = await service.BrowseAsync(result.Id, mode: BrowseMode.BrowseDirectChildren, cancellationToken: cancellationToken).ConfigureAwait(false);
                DIDLUtils.CopyItems(data["Result"], writer, containerIds, result.Depth + 1);
            }
        }
    }
}