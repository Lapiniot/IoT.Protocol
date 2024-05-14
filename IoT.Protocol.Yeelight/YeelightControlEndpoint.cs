using System.Collections.Concurrent;
using IoT.Protocol.Interfaces;
using OOs;
using OOs.IO.Pipelines;
using OOs.Memory;
using static System.Text.Json.JsonTokenType;
using static System.Threading.Tasks.TaskCreationOptions;

namespace IoT.Protocol.Yeelight;

[CLSCompliant(false)]
public sealed class YeelightControlEndpoint : PipeProcessor, IObservable<JsonElement>, IConnectedEndpoint<Command, JsonElement>
{
    private readonly ConcurrentDictionary<long, TaskCompletionSource<JsonElement>> completions = new();
    private readonly ObserversContainer<JsonElement> observers;
    private readonly Socket socket;
    private long counter;

    public YeelightControlEndpoint(IPEndPoint endpoint)
    {
        Endpoint = endpoint;
        observers = new();
        socket = new(Endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
    }

    public YeelightControlEndpoint(IPAddress address) : this(new IPEndPoint(address, 55443))
    {
    }

    public TimeSpan CommandTimeout { get; set; } = TimeSpan.FromSeconds(10);

    public IPEndPoint Endpoint { get; }

    #region Overrides of PipeConsumerCore

    /// <inheritdoc />
    protected override bool Consume(ref ReadOnlySequence<byte> buffer)
    {
        if (!buffer.TryReadLine(out var line))
        {
            return false;
        }

        buffer = buffer.Slice(line.Length + 2);
        var reader = new Utf8JsonReader(line.Span);
        while (reader.Read())
        {
            if (reader is not { TokenType: PropertyName, CurrentDepth: 1 }) continue;
            if (reader.ValueSpan.SequenceEqual("id"u8) && reader.Read() && reader.TryGetInt32(out var id))
            {
                if (!completions.TryRemove(id, out var completion) ||
                    !JsonReader.TryReadResult(ref reader, out var result, out var errorCode, out var errorMessage))
                {
                    return true;
                }

                if (result.ValueKind is JsonValueKind.Array or JsonValueKind.Object)
                {
                    completion.TrySetResult(result);
                }
                else if (errorMessage is not null)
                {
                    completion.TrySetException(new YeelightException(errorCode, errorMessage));
                }
                else
                {
                    completion.TrySetException(new YeelightException("Invalid operation result response"));
                }

                return true;
            }

            if (!reader.ValueSpan.SequenceEqual("method"u8) ||
                !reader.Read() || reader.TokenType is not JsonTokenType.String ||
                !reader.ValueSpan.SequenceEqual("props"u8))
            {
                continue;
            }

            while (reader.CurrentDepth >= 1 && reader.Read())
            {
                if (reader is not { TokenType: PropertyName, CurrentDepth: 1 })
                {
                    continue;
                }

                if (!reader.ValueSpan.SequenceEqual("params"u8)) continue;
                if (!reader.Read()) continue;

                observers.Notify(JsonElement.ParseValue(ref reader));
                return true;
            }

            return true;
        }

        return true;
    }

    #endregion

    #region Implementation of IConnectedEndpoint<RequestMessage, JsonElement>>

    public async Task<JsonElement> InvokeAsync(Command command, CancellationToken cancellationToken)
    {
        var completionSource = new TaskCompletionSource<JsonElement>(RunContinuationsAsynchronously);

        var id = Interlocked.Increment(ref counter);

        try
        {
            completions.TryAdd(id, completionSource);

            var buffer = ArrayPool<byte>.Shared.Rent(2048);

            try
            {
                var emitted = (int)command.WriteTo(buffer, id);
                buffer[emitted++] = SequenceExtensions.CR;
                buffer[emitted++] = SequenceExtensions.LF;

                await socket.SendAsync(buffer.AsMemory(0, emitted), SocketFlags.None, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }

            return await completionSource.Task.WaitAsync(CommandTimeout, cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            completionSource.TrySetCanceled(cancellationToken);
            throw;
        }
        finally
        {
            completions.TryRemove(id, out _);
        }
    }

    #endregion

    #region Implementation of IObservable<JsonElement>

    public IDisposable Subscribe(IObserver<JsonElement> observer) => observers.Subscribe(observer);

    #endregion

    #region Overrides of PipeProducerConsumer

    protected override ValueTask<int> ReceiveAsync(Memory<byte> buffer, CancellationToken cancellationToken) =>
        socket.ReceiveAsync(buffer, SocketFlags.None, cancellationToken);

    protected override async Task StartingAsync(CancellationToken cancellationToken)
    {
        await socket.ConnectAsync(Endpoint, cancellationToken).ConfigureAwait(false);
        await base.StartingAsync(cancellationToken).ConfigureAwait(false);
    }

    protected override async Task StoppingAsync()
    {
        try
        {
            await base.StoppingAsync().ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            // expected
        }
        finally
        {
            await socket.DisconnectAsync(true).ConfigureAwait(false);
        }
    }

    public override async ValueTask DisposeAsync()
    {
        using (socket)
        using (observers)
        {
            if (socket.Connected)
            {
                socket.Shutdown(SocketShutdown.Both);
            }

            await base.DisposeAsync().ConfigureAwait(false);
        }
    }

    #endregion

    #region Implementation of IConnectedObject

    public bool IsConnected => IsRunning;

    public Task ConnectAsync(CancellationToken cancellationToken = default) => StartActivityAsync(cancellationToken);

    public Task DisconnectAsync() => StopActivityAsync();

    #endregion
}