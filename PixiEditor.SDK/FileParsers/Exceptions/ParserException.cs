using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixiEditor.SDK.FileParsers
{

    [Serializable]
    public class ParserException : Exception
    {
        public Type ParserType { get; }

        public ParserException(Type parserType) : base("A parser threw an exception") => ParserType = parserType;

        public ParserException(Type parserType, string message) : base(message) => ParserType = parserType;

        public ParserException(Type parserType, string message, Exception inner) : base(message, inner) => ParserType = parserType;

        protected ParserException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
