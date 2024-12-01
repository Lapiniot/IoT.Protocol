using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Sockets;
using OOs.Policies;

namespace IoT.Protocol;

/// <summary>
/// Base abstract class for IoT devices enumerator which uses network discovery via UDP and performs 
/// active search by periodic emission of discovery datagrams 
/// </summary>
/// <typeparam name="TThing">Type of the 'thing' discoverable by concrete implementations</typeparam>
public abstract class UdpSearchEnumerator<TThing> : UdpEnumerator<TThing>
{
    private readonly IPEndPoint groupEndPoint;
    private readonly IRepeatPolicy searchRepeatPolicy;

    protected UdpSearchEnumerator(IPEndPoint groupEndPoint, IRepeatPolicy searchRepeatPolicy) :
        base((groupEndPoint ?? throw new ArgumentNullException(nameof(groupEndPoint))).AddressFamily)
    {
        ArgumentNullException.ThrowIfNull(searchRepeatPolicy);
        this.groupEndPoint = groupEndPoint;
        this.searchRepeatPolicy = searchRepeatPolicy;
    }

    protected IPEndPoint GroupEndPoint => groupEndPoint;

    protected sealed override Task StartAuxiliaryWorkerAsync([NotNull] Socket socket, CancellationToken stoppingToken)
    {
        var message = CreateDiscoveryDatagram();
        return searchRepeatPolicy.RepeatAsync(SendDiscoveryDatagramAsync, stoppingToken);

        async Task SendDiscoveryDatagramAsync(CancellationToken token) =>
            await socket.SendToAsync(message, SocketFlags.None, groupEndPoint, token).ConfigureAwait(false);
    }

    protected abstract ReadOnlyMemory<byte> CreateDiscoveryDatagram();
}