using System;
using System.Runtime.Serialization;

namespace IoT.Protocol.Soap
{
    [System.Serializable]
    public class SoapException : System.Exception
    {
        public SoapException() { }
        public SoapException(string message) : base(message) {}

        public SoapException(string message, Exception innerException) : base(message, innerException) {}

        public SoapException(string message, System.Exception inner, int code, string description) : base(message, inner)
        {
            Code = code;
            Description = description;
        }
        protected SoapException(SerializationInfo info, StreamingContext context) : base(info, context) {}

        public int Code { get; }
        public string Description { get; }
    }
}