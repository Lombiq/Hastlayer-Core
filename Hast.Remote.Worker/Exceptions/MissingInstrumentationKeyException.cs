using System;

namespace Hast.Remote.Worker.Exceptions
{
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
    }
}
