namespace IoT.Protocol.Interfaces;

public interface IControlEndpoint<in TRequest, TResponse>
{
    Task<TResponse> InvokeAsync(TRequest message, CancellationToken cancellationToken);
}