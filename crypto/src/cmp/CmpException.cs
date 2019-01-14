﻿using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Org.BouncyCastle.Cmp
{
    public class CmpException : Exception
    {
        public CmpException()
        {
        }

        public CmpException(string message) : base(message)
        {
        }

        public CmpException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected CmpException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
