using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Sockets;
using System.Policies;

namespace IoT.Protocol;

/// <summary>
/// Base abstract class for IoT devices enumerator which uses network discovery via UDP and performs 
/// active search by periodic emition of discovery datagrams 
/// </summary>
/// <typeparam name="TThing">Type of the 'thing' discoverable by concrete implementations</typeparam>
public abstract class UdpSearchEnumerator<TThing> : UdpEnumerator<TThing>
{
    private readonly IPEndPoint groupEP;
    private readonly IRepeatPolicy repeatPolicy;

    protected UdpSearchEnumerator(IPEndPoint groupEP, IRepeatPolicy repeatPolicy) :
        base((groupEP ?? throw new ArgumentNullException(nameof(groupEP))).AddressFamily)
    {
        ArgumentNullException.ThrowIfNull(repeatPolicy);
        this.groupEP = groupEP;
        this.repeatPolicy = repeatPolicy;
    }

    protected IPEndPoint GroupEP => groupEP;

    protected sealed override Task StartAuxiliaryWorkerAsync([NotNull] Socket socket, CancellationToken stoppingToken)
    {
        using var memory = MemoryPool<byte>.Shared.Rent(socket.SendBufferSize);
        WriteDiscoveryDatagram(memory.Memory.Span, out var written);
        var message = memory.Memory[..written];

        return repeatPolicy.RepeatAsync(SendDiscoveryDatagramAsync, stoppingToken);

        async ValueTask SendDiscoveryDatagramAsync(CancellationToken token)
        {
            var vt = socket.SendToAsync(message, SocketFlags.None, groupEP, token);
            if(!vt.IsCompletedSuccessfully)
            {
                await vt.ConfigureAwait(false);
            }
        }
    }

    /// <summary>
    /// Writes datagram bytes to be send over the network for discovery
    /// </summary>
    protected abstract void WriteDiscoveryDatagram(Span<byte> span, out int bytesWritten);
}