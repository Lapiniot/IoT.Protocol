using System.Buffers;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Memory;
using System.Net;
using System.Net.Pipelines;
using System.Net.Sockets;
using System.Text.Json;
using IoT.Protocol.Interfaces;
using static System.Buffers.ArrayPool<byte>;
using static System.Net.Sockets.SocketFlags;
using static System.TimeSpan;

namespace IoT.Protocol.Yeelight;

[CLSCompliant(false)]
public class YeelightControlEndpoint : PipeProducerConsumer, IObservable<JsonElement>, IConnectedEndpoint<RequestMessage, JsonElement>
{
    private readonly ConcurrentDictionary<long, TaskCompletionSource<JsonElement>> completions = new();
    private readonly ObserversContainer<JsonElement> observers;
    private long counter;
    private Socket socket;

    public YeelightControlEndpoint(uint deviceId, IPEndPoint endpoint)
    {
        Endpoint = endpoint;
        DeviceId = deviceId;
        observers = new ObserversContainer<JsonElement>();
    }

    public uint DeviceId { get; }

    protected TimeSpan CommandTimeout { get; } = FromSeconds(10);

    public IPEndPoint Endpoint { get; }

    #region Implementation of IControlEndpoint<in IDictionary<string,object>,IDictionary<string,object>>

    public async Task<JsonElement> InvokeAsync(RequestMessage message, CancellationToken cancellationToken)
    {
        var completionSource = new TaskCompletionSource<JsonElement>(cancellationToken);

        var id = Interlocked.Increment(ref counter);

        try
        {
            _ = completions.TryAdd(id, completionSource);

            var buffer = Shared.Rent(2048);

            try
            {
                var emitted = (int)message.SerializeTo(buffer, id);
                buffer[emitted] = SequenceExtensions.CR;
                buffer[emitted + 1] = SequenceExtensions.LF;
                var vt = socket.SendAsync(buffer.AsMemory(0, emitted + 2), None, cancellationToken);
                if(!vt.IsCompletedSuccessfully) _ = await vt.ConfigureAwait(false);
            }
            finally
            {
                Shared.Return(buffer);
            }

            using var timeoutSource = new CancellationTokenSource(CommandTimeout);

            return await completionSource.Task.WaitAsync(timeoutSource.Token).ConfigureAwait(false);
        }
        catch(OperationCanceledException)
        {
            _ = completionSource.TrySetCanceled(cancellationToken);
            throw;
        }
        finally
        {
            _ = completions.TryRemove(id, out _);
        }
    }

    #endregion

    #region Implementation of IObservable<out IDictionary<string,object>>

    public IDisposable Subscribe(IObserver<JsonElement> observer)
    {
        return observers.Subscribe(observer);
    }

    #endregion

    private static void OnDisconnectAsyncCompleted(object sender, SocketAsyncEventArgs e)
    {
        var completionSource = (TaskCompletionSource<bool>)e.UserToken;

        if(e.SocketError != SocketError.Success)
        {
            completionSource.SetException(new SocketException((int)e.SocketError));
        }
        else
        {
            completionSource.SetResult(true);
        }
    }

    #region Overrides of PipeProducerConsumer

    protected override ValueTask<int> ReceiveAsync(Memory<byte> buffer, CancellationToken cancellationToken)
    {
        return socket.ReceiveAsync(buffer, None, cancellationToken);
    }

    protected override long Consume(in ReadOnlySequence<byte> buffer)
    {
        if(!buffer.TryReadLine(out var line)) return 0;

        var consumed = line.Length + 2;

        try
        {
            var message = JsonSerializer.Deserialize<JsonElement>(line.Span);

            if(message.TryGetProperty("id", out var id))
            {
                if(completions.TryRemove(id.GetInt64(), out var completion))
                {
                    _ = completion.TrySetResult(message);
                }
            }
            else if(message.TryGetProperty("method", out var m) && m.GetString() == "props" &&
                    message.TryGetProperty("params", out var p))
            {
                observers.Notify(p);
            }
        }
        catch(Exception e)
        {
            Trace.TraceError($"Error processing response message: {e.Message}");
            throw;
        }

        return consumed;
    }

    protected override async Task StartingAsync(CancellationToken cancellationToken)
    {
        socket = new Socket(Endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        await socket.ConnectAsync(Endpoint, cancellationToken).ConfigureAwait(false);
        await base.StartingAsync(cancellationToken).ConfigureAwait(false);
    }

    protected override async Task StoppingAsync()
    {
        await base.StoppingAsync().ConfigureAwait(false);

        var tcs = new TaskCompletionSource<bool>();

        var args = new SocketAsyncEventArgs { UserToken = tcs };

        args.Completed += OnDisconnectAsyncCompleted;

        try
        {
            _ = socket.DisconnectAsync(args);
            _ = await tcs.Task.ConfigureAwait(false);
        }
        finally
        {
            args.Completed -= OnDisconnectAsyncCompleted;
            args.Dispose();
        }
    }

    #endregion

    #region Implementation of IConnectedObject

    public bool IsConnected => IsRunning;

    public Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        return StartActivityAsync(cancellationToken);
    }

    public Task DisconnectAsync()
    {
        return StopActivityAsync();
    }

    #endregion
}