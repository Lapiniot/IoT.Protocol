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
        public static readonly XNamespace NS = "urn:schemas-upnp-org:service-1-0";

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
            var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);

            await using(stream.ConfigureAwait(false))
            {
                var x = XDocument.Load(stream);

                var stateTable = x.Root.Element(NS + "serviceStateTable").Elements(NS + "stateVariable").ToDictionary(
                    sv => sv.Element(NS + "name").Value,
                    sv => new StateVariable(
                        sv.Element(NS + "name").Value,
                        CreateType(sv.Element(NS + "dataType")),
                        sv.Element(NS + "defaultValue")?.Value,
                        sv.Attribute(NS + "sendEvents")?.Value != "no",
                        sv.Element(NS + "allowedValueList")?.Elements(NS + "allowedValue").Select(av => av.Value).ToArray(),
                        CreateRange(sv.Element(NS + "allowedValueRange"))));

                var actions = x.Root.Element(NS + "actionList").Elements(NS + "action").Select(a => new ServiceAction(
                    a.Element(NS + "name").Value,
                    a.Element(NS + "argumentList").Elements(NS + "argument").Select(arg => new Argument(
                        arg.Element(NS + "name").Value,
                        arg.Element(NS + "direction").Value,
                        arg.Element(NS + "retval") != null,
                        stateTable[arg.Element(NS + "relatedStateVariable").Value]
                    )).ToArray()
                )).ToArray();

                return new ServiceMetadata(actions, stateTable);
            }
        }

        private static string CreateType(XElement element)
        {
            return element.Attribute(NS + "type")?.Value ?? element.Value;
        }

        private static ArgumentValueRange CreateRange(XContainer element)
        {
            return element != null
                ? new ArgumentValueRange(
                    element.Element(NS + "minimum").Value,
                    element.Element(NS + "maximum").Value,
                    element.Element(NS + "step").Value)
                : null;
        }
    }
}