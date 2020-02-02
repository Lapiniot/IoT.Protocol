using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace IoT.Protocol.Upnp.Metadata
{
    public class ServiceMetadata
    {
        private static readonly XNamespace Ns = "urn:schemas-upnp-org:service-1-0";

        internal ServiceMetadata(IEnumerable<ServiceAction> actions, IReadOnlyDictionary<string, StateVariable> stateTable)
        {
            Actions = actions;
            StateTable = stateTable;
        }

        public IEnumerable<ServiceAction> Actions { get; }

        public IReadOnlyDictionary<string, StateVariable> StateTable { get; }

        public static async Task<ServiceMetadata> LoadAsync(Uri location, CancellationToken cancellationToken)
        {
            using var client = new HttpClient();
            using var response = await client.GetAsync(location, cancellationToken).ConfigureAwait(false);
            await using var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            var x = XDocument.Load(stream);

            var stateTable = x.Root.Element(Ns + "serviceStateTable").Elements(Ns + "stateVariable").ToDictionary(
                sv => sv.Element(Ns + "name").Value,
                sv => new StateVariable(
                    sv.Element(Ns + "name").Value,
                    CreateType(sv.Element(Ns + "dataType")),
                    sv.Element(Ns + "defaultValue")?.Value,
                    sv.Attribute(Ns + "sendEvents")?.Value != "no",
                    sv.Element(Ns + "allowedValueList")?.Elements(Ns + "allowedValue").Select(av => av.Value).ToArray(),
                    CreateRange(sv.Element(Ns + "allowedValueRange"))));

            var actions = x.Root.Element(Ns + "actionList").Elements(Ns + "action").Select(a => new ServiceAction(
                a.Element(Ns + "name").Value,
                a.Element(Ns + "argumentList").Elements(Ns + "argument").Select(arg => new Argument(
                    arg.Element(Ns + "name").Value,
                    arg.Element(Ns + "direction").Value,
                    arg.Element(Ns + "retval") != null,
                    stateTable[arg.Element(Ns + "relatedStateVariable").Value]
                )).ToArray()
            )).ToArray();

            return new ServiceMetadata(actions, stateTable);
        }

        private static string CreateType(XElement element)
        {
            return element.Attribute(Ns + "type")?.Value ?? element.Value;
        }

        private static ArgumentValueRange CreateRange(XContainer element)
        {
            return element != null
                ? new ArgumentValueRange(
                    element.Element(Ns + "minimum").Value,
                    element.Element(Ns + "maximum").Value,
                    element.Element(Ns + "step").Value)
                : null;
        }
    }
}