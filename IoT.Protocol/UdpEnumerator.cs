using System.Buffers;
using System.Net;
using System.Net.Sockets;

namespace IoT.Protocol;

/// <summary>
/// Base abstract class for IoT devices enumerator which uses network discovery via UDP
/// </summary>
/// <typeparam name="TThing">Type of the 'thing' discoverable by concrete implementations</typeparam>
public abstract class UdpEnumerator<TThing> : IAsyncEnumerable<TThing>
{
    private readonly AddressFamily addressFamily;

    protected UdpEnumerator(AddressFamily addressFamily)
    {
        if(addressFamily is not (AddressFamily.InterNetworkV6 or AddressFamily.InterNetwork))
        {
            throw new ArgumentNullException($"Only '{AddressFamily.InterNetwork}' or '{AddressFamily.InterNetworkV6}' are supported as address family");
        }

        this.addressFamily = addressFamily;
    }

    #region Implementation of IAsyncEnumerable<out TThing>

    public virtual async IAsyncEnumerator<TThing> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        using var socket = new Socket(addressFamily, SocketType.Dgram, ProtocolType.Udp);

        ConfigureSocket(socket, out IPEndPoint receiveEndPoint);

        using var memory = MemoryPool<byte>.Shared.Rent(socket.ReceiveBufferSize);
        var buffer = memory.Memory;

        while(!cancellationToken.IsCancellationRequested)
        {
            TThing instance = default;

            try
            {
                var rvt = socket.ReceiveFromAsync(buffer, SocketFlags.None, receiveEndPoint, cancellationToken);
                var result = rvt.IsCompletedSuccessfully ? rvt.Result : await rvt.ConfigureAwait(false);
                var cvt = ParseDatagramAsync(buffer[..result.ReceivedBytes], (IPEndPoint)result.RemoteEndPoint, cancellationToken);
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
    /// Configures UDP dgram socket instance
    /// </summary>
    /// <param name="socket">Datagram receiver socket instance to be configured</param>
    /// <param name="receiveEndPoint">Remote network endpoint to receive datagrams from</param>
    protected abstract void ConfigureSocket(Socket socket, out IPEndPoint receiveEndPoint);

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
    protected abstract ValueTask<TThing> ParseDatagramAsync(ReadOnlyMemory<byte> buffer, IPEndPoint remoteEp, CancellationToken cancellationToken);
}