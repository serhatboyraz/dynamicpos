using System;

namespace DynamicPos.Ingenico
{
    public class CrdException:Exception
    {
        public CrdException()
            : base()
        {

        }

        public CrdException(string message)
            : base(message)
        {

        }

        public CrdException(string message, Exception innerException)
            : base(message, innerException)
        {

        }

        private CrdException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {

        }

    }
}
