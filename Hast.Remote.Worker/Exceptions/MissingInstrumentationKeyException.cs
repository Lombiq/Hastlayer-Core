using System;
using System.Runtime.Serialization;

namespace Hast.Remote.Worker.Exceptions;

[Serializable]
public class MissingInstrumentationKeyException : Exception
{
    public MissingInstrumentationKeyException()
    {
    }

    public MissingInstrumentationKeyException(string message)
        : base(message)
    {
    }

    public MissingInstrumentationKeyException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    protected MissingInstrumentationKeyException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
