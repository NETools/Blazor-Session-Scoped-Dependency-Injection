using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorSessionScopedContainer.Misc.Json
{
    enum TokenType
    {
        BracketOpen,
        BracketClose,
        Comma,
        QuoteSign,
        Colon,
        String
    };

    internal class JsonToken
    {
        public TokenType TokenType;
        public string Value;

        public override string ToString()
        {
            return $"[{TokenType}] {Value}";
        }
    }
}
