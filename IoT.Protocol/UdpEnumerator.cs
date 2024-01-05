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
        if (addressFamily is not (AddressFamily.InterNetworkV6 or AddressFamily.InterNetwork))
        {
            throw new ArgumentNullException($"Only '{AddressFamily.InterNetwork}' or '{AddressFamily.InterNetworkV6}' are supported as address family");
        }

        this.addressFamily = addressFamily;
    }

    #region Implementation of IAsyncEnumerable<out TThing>

    public virtual async IAsyncEnumerator<TThing> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        using var socket = new Socket(addressFamily, SocketType.Dgram, ProtocolType.Udp);

        ConfigureSocket(socket, out var receiveEndPoint);

        var auxWorker = StartAuxiliaryWorkerAsync(socket, cancellationToken);

        using var memory = MemoryPool<byte>.Shared.Rent(socket.ReceiveBufferSize);
        var buffer = memory.Memory;

        while (!cancellationToken.IsCancellationRequested)
        {
            bool success;
            TThing instance;

            try
            {
                var result = await socket.ReceiveFromAsync(buffer, SocketFlags.None, receiveEndPoint, cancellationToken).ConfigureAwait(false);
                success = TryParseDatagram(buffer[..result.ReceivedBytes], (IPEndPoint)result.RemoteEndPoint, out instance);
            }
            catch (OperationCanceledException)
            {
                // expected external cancellation signal
                yield break;
            }

            if (success)
            {
                yield return instance;
            }
        }

        try
        {
            await auxWorker.ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            // Expected
        }
    }

    #endregion

    /// <summary>
    /// Denotes auxiliary worker task has to be started and run in parallel with the main datagram receiver loop.
    /// </summary>
    /// <param name="socket">UDP datagram socket instance, same as it is currently used in the iterator receiver loop</param>
    /// <param name="stoppingToken">Stopping <see cref="CancellationToken" /> for external cancellation notification</param>
    /// <returns><see cref="Task" /> which represents current asynchronous worker task</returns>
    /// <remarks>
    /// Implementors may provide any custom logic performed on socket instance. 
    /// This can be periodic discovery datagram emission e.g., in case discovery protocol supposes active searching. 
    /// </remarks>
    protected virtual Task StartAuxiliaryWorkerAsync(Socket socket, CancellationToken stoppingToken) => Task.CompletedTask;

    /// <summary>
    /// Configures UDP dgram socket instance
    /// </summary>
    /// <param name="socket">Datagram receiver socket instance to be configured</param>
    /// <param name="receiveEndPoint">Remote network endpoint to receive datagrams from</param>
    protected abstract void ConfigureSocket(Socket socket, out IPEndPoint receiveEndPoint);

    /// <summary>
    /// Factory method to create IoT device instance by parsing discovery response datagram bytes.
    /// </summary>
    /// <param name="buffer">Buffer containing message.</param>
    /// <param name="remoteEndPoint">Responder endpoint information.</param>
    /// <param name="thing">Instance of type <typeparamref name="TThing"/>.</param>
    /// <returns><see langword="true" /> if data in the buffer was succesfully parsed.</returns>
    protected abstract bool TryParseDatagram(ReadOnlyMemory<byte> buffer, IPEndPoint remoteEndPoint, out TThing thing);
}