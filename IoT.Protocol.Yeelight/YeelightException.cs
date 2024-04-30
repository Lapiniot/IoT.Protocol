using IoT.Protocol.Exceptions;

namespace IoT.Protocol.Yeelight;

[CLSCompliant(true)]
public sealed class YeelightException : ProtocolException
{
    public YeelightException() { }

    public YeelightException(string message) : base(message) { }

    public YeelightException(int code, string message) : base(code, message) { }

    public YeelightException(string message, Exception innerException) :
        base(message, innerException)
    { }

    public YeelightException(int code, string message, Exception innerException) :
        base(code, message, innerException)
    { }
}