using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EsoLang
{
    public class EsoException : Exception
    {
        public EsoException()
        {
        }

        public EsoException(string message) : base(message)
        {
        }

        public EsoException(string message, Exception inner) : base(message, inner)
        {
        }
    }

    public class ParseException : EsoException
    {
        public ParseException()
        {
        }

        public ParseException(string message) : base(message)
        {
        }

        public ParseException(string message, Exception inner) : base(message, inner)
        {
        }
    }

    public class InterpException : EsoException
    {
        public InterpException()
        {
        }

        public InterpException(string message) : base(message)
        {
        }

        public InterpException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
