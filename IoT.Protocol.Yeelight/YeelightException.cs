using System.Runtime.Serialization;

namespace IoT.Protocol.Yeelight;

[CLSCompliant(true)]
public class YeelightException : Exception
{
    public YeelightException(int code, string message, Exception innerException) :
        base(message, innerException)
    {
        Code = code;
    }

    public YeelightException(int code, string message) : this(code, message, null) { }

    protected YeelightException(SerializationInfo info, StreamingContext context) : base(info, context) { }

    public YeelightException(string message) : base(message) { }

    public YeelightException(string message, Exception innerException) : base(message, innerException) { }

    public YeelightException() : this(0, "Unknown error") { }

    public int Code { get; }
}