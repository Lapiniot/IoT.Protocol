namespace IoT.Protocol.Exceptions;

[CLSCompliant(true)]
public abstract class ProtocolException : Exception
{
    protected ProtocolException(int code, string message, Exception? innerException) :
        base(message, innerException) => Code = code;

    protected ProtocolException(int code, string message) : this(code, message, null) { }

    protected ProtocolException(string message) : base(message) { }

    protected ProtocolException(string message, Exception innerException) : base(message, innerException) { }

    protected ProtocolException() : this(0, "Unknown error") { }

    public int Code { get; }
}