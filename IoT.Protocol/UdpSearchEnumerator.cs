using System.Net;
using System.Net.Sockets;
using System.Policies;

namespace IoT.Protocol;

/// <summary>
/// Base abstract class for IoT devices enumerator which uses network discovery via UDP
/// </summary>
/// <typeparam name="TThing">Type of the 'thing' discoverable by concrete implementations</typeparam>
public abstract class UdpSearchEnumerator<TThing> : IAsyncEnumerable<TThing>
{
    private readonly CreateSocketFactory createSocket;
    private readonly IRepeatPolicy discoveryPolicy;
    private readonly bool distinctAddress;

    protected UdpSearchEnumerator(CreateSocketFactory createSocketFactory, IPEndPoint groupEndpoint,
        bool distinctAddress, IRepeatPolicy discoveryPolicy)
    {
        createSocket = createSocketFactory;
        GroupEndpoint = groupEndpoint;
        this.distinctAddress = distinctAddress;
        this.discoveryPolicy = discoveryPolicy;
    }

    protected abstract int SendBufferSize { get; }
    protected abstract int ReceiveBufferSize { get; }
    public IPEndPoint GroupEndpoint { get; }

    #region Implementation of IAsyncEnumerable<out TThing>

    public async IAsyncEnumerator<TThing> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        var addresses = new HashSet<IPAddress>(EqualityComparer<IPAddress>.Default);

        using var socket = createSocket(GroupEndpoint);
        socket.ReceiveBufferSize = ReceiveBufferSize;

        Memory<byte> buffer = new byte[ReceiveBufferSize];

        if(SendBufferSize > 0)
        {
            socket.SendBufferSize = SendBufferSize;
            var datagram = new byte[SendBufferSize];
            WriteDiscoveryDatagram(datagram, out var written);

            if(written > 0)
            {
                var message = datagram.AsMemory(0, written);
                var _ = discoveryPolicy.RepeatAsync(SendDiscoveryDatagramAsync, cancellationToken);

                async ValueTask SendDiscoveryDatagramAsync(CancellationToken token)
                {
                    var vt = socket.SendToAsync(message, SocketFlags.None, GroupEndpoint, token);
                    if(!vt.IsCompletedSuccessfully)
                    {
                        await vt.ConfigureAwait(false);
                    }
                }
            }
        }

        while(!cancellationToken.IsCancellationRequested)
        {
            TThing instance = default;

            try
            {
                var rvt = socket.ReceiveFromAsync(buffer, SocketFlags.None, GroupEndpoint, cancellationToken);
                var result = rvt.IsCompletedSuccessfully ? rvt.Result : await rvt.ConfigureAwait(false);

                if(distinctAddress && !addresses.Add(((IPEndPoint)result.RemoteEndPoint).Address)) continue;

                var cvt = CreateInstanceAsync(buffer[..result.ReceivedBytes], (IPEndPoint)result.RemoteEndPoint, cancellationToken);
                instance = cvt.IsCompletedSuccessfully ? cvt.Result : await cvt.ConfigureAwait(false);
            }
            catch(OperationCanceledException oce) when(oce.CancellationToken == cancellationToken)
            {
                // expected external cancellation signal
                yield break;
            }
            catch(OperationCanceledException)
            {
                // ignored by design: this can be cancellation comming 
                // from CreateInstanceAsync internals (some timeout e.g.)
                continue;
            }
            catch(InvalidDataException)
            {
                // ignored as expected if received datagram has wrong format
                continue;
            }

            yield return instance;
        }
    }

    #endregion

    /// <summary>
    /// Factory method to create IoT device instance by parsing discovery response datagram bytes
    /// </summary>
    /// <param name="buffer">Buffer containing message</param>
    /// <param name="remoteEp">Responder endpoint information</param>
    /// <param name="cancellationToken"></param>
    /// <returns>
    /// Instance of type
    /// <typeparam name="TThing" />
    /// </returns>
    protected abstract ValueTask<TThing> CreateInstanceAsync(ReadOnlyMemory<byte> buffer, IPEndPoint remoteEp, CancellationToken cancellationToken);

    /// <summary>
    /// Returns datagram bytes to be send over the network for discovery
    /// </summary>
    /// <returns>Raw datagram bytes</returns>
    protected abstract void WriteDiscoveryDatagram(Span<byte> span, out int bytesWritten);
}