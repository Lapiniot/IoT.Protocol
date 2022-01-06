namespace IoT.Protocol.Interfaces;

public interface IControlEndpoint<in TCommand, TResult>
{
    Task<TResult> InvokeAsync(TCommand command, CancellationToken cancellationToken);
}

public interface IControlEndpoint<in TCommand>
{
    Task InvokeAsync(TCommand command, CancellationToken cancellationToken);
}