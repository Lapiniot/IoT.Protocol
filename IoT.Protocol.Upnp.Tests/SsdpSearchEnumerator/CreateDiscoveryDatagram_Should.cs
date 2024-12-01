using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using OOs.Net.Sockets;

namespace IoT.Protocol.Upnp.Tests.SsdpSearchEnumerator;

[TestClass]
public class CreateDiscoveryDatagram_Should
{
    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod)]
    private static extern ReadOnlyMemory<byte> CreateDiscoveryDatagram(
        Upnp.SsdpSearchEnumerator? enumerator, IPEndPoint groupEndPoint,
        string searchTarget, string userAgent);

    [TestMethod]
    public void ReturnFormattedMessageDatagram_GivenIPv4AddressEndPoint()
    {
        var expected = "M-SEARCH * HTTP/1.1\r\nHOST: 239.255.255.250:1900\r\nMAN: \"ssdp:discover\"\r\nMX: 1\r\nST: upnp:rootdevice\r\nUSER-AGENT: SsdpSearchEnumerator/1.9.0.0\r\n\r\n"u8;

        var actual = CreateDiscoveryDatagram(null,
            groupEndPoint: SocketBuilderExtensions.GetIPv4SSDPGroup(),
            searchTarget: "upnp:rootdevice",
            userAgent: "SsdpSearchEnumerator/1.9.0.0");

        Assert.IsTrue(expected.SequenceEqual(actual.Span));
    }

    [TestMethod]
    public void ReturnFormattedMessageDatagram_GivenIPv6AddressEndPoint()
    {
        var expected = "M-SEARCH * HTTP/1.1\r\nHOST: [ff02::c]:1900\r\nMAN: \"ssdp:discover\"\r\nMX: 1\r\nST: upnp:rootdevice\r\nUSER-AGENT: SsdpSearchEnumerator/1.9.0.0\r\n\r\n"u8;

        var actual = CreateDiscoveryDatagram(null,
            groupEndPoint: SocketBuilderExtensions.GetIPv6SSDPGroup(),
            searchTarget: "upnp:rootdevice",
            userAgent: "SsdpSearchEnumerator/1.9.0.0");
        Console.WriteLine(Encoding.UTF8.GetString(actual.Span));

        Assert.IsTrue(expected.SequenceEqual(actual.Span));
    }
}