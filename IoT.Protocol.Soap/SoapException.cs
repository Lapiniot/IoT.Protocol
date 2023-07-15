namespace IoT.Protocol.Soap;

public sealed class SoapException : Exception
{
    public SoapException() { }
    public SoapException(string message) : base(message) { }

    public SoapException(string message, Exception innerException) : base(message, innerException) { }

    public SoapException(string message, Exception inner, int code, string description) : base(message, inner)
    {
        Code = code;
        Description = description;
    }

    public int Code { get; }
    public string Description { get; }
}