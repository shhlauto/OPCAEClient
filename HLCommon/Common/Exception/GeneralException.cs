using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HLCommon
{
    public class GeneralException : ApplicationException
    {
        public GeneralException() { }
        public GeneralException(string message) : base(message) { }
        public GeneralException(string message, Exception inner)
            : base(message, inner) { }
        public GeneralException(System.Runtime.Serialization.SerializationInfo info,
        System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
