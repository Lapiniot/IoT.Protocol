using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Sockets;
using System.Policies;

namespace IoT.Protocol;

/// <summary>
/// Base abstract class for IoT devices enumerator which uses network discovery via UDP and performs 
/// active search by periodic emission of discovery datagrams 
/// </summary>
/// <typeparam name="TThing">Type of the 'thing' discoverable by concrete implementations</typeparam>
public abstract class UdpSearchEnumerator<TThing> : UdpEnumerator<TThing>
{
    private readonly IPEndPoint groupEndPoint;
    private readonly IRepeatPolicy repeatPolicy;

    protected UdpSearchEnumerator(IPEndPoint groupEndPoint, IRepeatPolicy repeatPolicy) :
        base((groupEndPoint ?? throw new ArgumentNullException(nameof(groupEndPoint))).AddressFamily)
    {
        ArgumentNullException.ThrowIfNull(repeatPolicy);
        this.groupEndPoint = groupEndPoint;
        this.repeatPolicy = repeatPolicy;
    }

    protected IPEndPoint GroupEndPoint => groupEndPoint;

    protected sealed override Task StartAuxiliaryWorkerAsync([NotNull] Socket socket, CancellationToken stoppingToken)
    {
        using var memory = MemoryPool<byte>.Shared.Rent(socket.SendBufferSize);
        WriteDiscoveryDatagram(memory.Memory.Span, out var written);
        var message = memory.Memory[..written];

        return repeatPolicy.RepeatAsync(SendDiscoveryDatagramAsync, stoppingToken);

        async Task SendDiscoveryDatagramAsync(CancellationToken token) =>
            await socket.SendToAsync(message, SocketFlags.None, groupEndPoint, token).ConfigureAwait(false);
    }

    /// <summary>
    /// Writes datagram bytes to be send over the network for discovery
    /// </summary>
    protected abstract void WriteDiscoveryDatagram(Span<byte> span, out int bytesWritten);
}