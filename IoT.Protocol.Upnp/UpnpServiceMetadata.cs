using System;
using System.Threading;
using System.Threading.Tasks;

namespace IoT.Protocol.Upnp
{
    public class UpnpServiceMetadata
    {

        internal UpnpServiceMetadata(UpnpServiceActionMetadata[] actions)
        {
            this.Actions = actions;
        }

        public UpnpServiceActionMetadata[] Actions { get; }
    }
}