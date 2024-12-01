namespace IoT.Protocol.Upnp.Tests.SsdpReply;

[TestClass]
public class TryParse_Should
{
    [TestMethod]
    public void ReturnTrueAndNonEmptyReply_GivenValidSample()
    {
        var data = "M-SEARCH * HTTP/1.1\r\nHOST: 239.255.255.250:1900\r\nMAN: \"ssdp:discover\"\r\nMX: 1\r\nST: upnp:rootdevice\r\nUSER-AGENT: SsdpSearchEnumerator/1.9.0.0 (Darwin 24.1.0 Darwin Kernel Version 24.1.0: Thu Oct 10 21:02:27 PDT 2024; root:xnu-11215.41.3~2/RELEASE_X86_64)\r\n\r\n"u8;

        var actual = Upnp.SsdpReply.TryParse(data, out var reply);

        Assert.IsTrue(actual);
        Assert.IsNotNull(reply);
        Assert.AreEqual(5, reply.Count);
        Assert.AreEqual("M-SEARCH * HTTP/1.1", reply.StartLine);
        Assert.AreEqual("239.255.255.250:1900", reply["HOST"]);
        Assert.AreEqual("\"ssdp:discover\"", reply["MAN"]);
        Assert.AreEqual("1", reply["MX"]);
        Assert.AreEqual("upnp:rootdevice", reply["ST"]);
        Assert.AreEqual("SsdpSearchEnumerator/1.9.0.0 (Darwin 24.1.0 Darwin Kernel Version 24.1.0: Thu Oct 10 21:02:27 PDT 2024; root:xnu-11215.41.3~2/RELEASE_X86_64)", reply["USER-AGENT"]);
    }

    [TestMethod]
    public void ReturnTrueAndNonEmptyReply_StopProcessingHeadersOnEmptyLine_GivenValidSampleWithBody()
    {
        var data = "M-SEARCH * HTTP/1.1\r\nHOST: 239.255.255.250:1900\r\nMAN: \"ssdp:discover\"\r\nMX: 1\r\n\r\nST: upnp:rootdevice\r\nUSER-AGENT: SsdpSearchEnumerator/1.9.0.0 (Darwin 24.1.0 Darwin Kernel Version 24.1.0: Thu Oct 10 21:02:27 PDT 2024; root:xnu-11215.41.3~2/RELEASE_X86_64)\r\n\r\n"u8;

        var actual = Upnp.SsdpReply.TryParse(data, out var reply);

        Assert.IsTrue(actual);
        Assert.IsNotNull(reply);
        Assert.AreEqual(3, reply.Count);
        Assert.AreEqual("M-SEARCH * HTTP/1.1", reply.StartLine);
        Assert.AreEqual("239.255.255.250:1900", reply["HOST"]);
        Assert.AreEqual("\"ssdp:discover\"", reply["MAN"]);
        Assert.AreEqual("1", reply["MX"]);
    }

    [TestMethod]
    public void ReturnFalseAndNullReply_GivenEmptySpanSample()
    {
        ReadOnlySpan<byte> data = [];

        var actual = Upnp.SsdpReply.TryParse(data, out var reply);

        Assert.IsFalse(actual);
        Assert.IsNull(reply);
    }

    [TestMethod]
    public void ReturnFalseAndNullReply_GivenSampleWithEmptyFirstLine()
    {
        var data = "\r\n"u8;

        var actual = Upnp.SsdpReply.TryParse(data, out var reply);

        Assert.IsFalse(actual);
        Assert.IsNull(reply);
    }

    [TestMethod]
    public void ReturnFalseAndNullReply_GivenInvalidSampleWithNonHeaderPairLines()
    {
        var data = "M-SEARCH * HTTP/1.1\r\nHOST: 239.255.255.250:1900\r\nnon-header-line!!!\r\nMX: 1\r\nST: upnp:rootdevice\r\nUSER-AGENT: SsdpSearchEnumerator/1.9.0.0 (Darwin 24.1.0 Darwin Kernel Version 24.1.0: Thu Oct 10 21:02:27 PDT 2024; root:xnu-11215.41.3~2/RELEASE_X86_64)\r\n\r\n"u8;

        var actual = Upnp.SsdpReply.TryParse(data, out var reply);

        Assert.IsFalse(actual);
        Assert.IsNull(reply);
    }

    [TestMethod]
    public void ReturnFalseAndNullReply_GivenInvalidSampleWithMissingHeaderNameLine()
    {
        var data = "M-SEARCH * HTTP/1.1\r\nHOST: 239.255.255.250:1900\r\nMX: 1\r\n: missing header name here\r\nST: upnp:rootdevice\r\nUSER-AGENT: SsdpSearchEnumerator/1.9.0.0 (Darwin 24.1.0 Darwin Kernel Version 24.1.0: Thu Oct 10 21:02:27 PDT 2024; root:xnu-11215.41.3~2/RELEASE_X86_64)\r\n\r\n"u8;

        var actual = Upnp.SsdpReply.TryParse(data, out var reply);

        Assert.IsFalse(actual);
        Assert.IsNull(reply);
    }
}