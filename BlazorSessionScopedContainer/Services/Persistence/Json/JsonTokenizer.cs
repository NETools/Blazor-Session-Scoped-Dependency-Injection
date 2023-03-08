using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorSessionScopedContainer.Services.Persistence.Json
{
    internal static class JsonTokenizer
    {
        public static IEnumerable<JsonToken> NormalizeTokenStream(IEnumerable<JsonToken> tokens)
        {
            var tokenStream = tokens.GetEnumerator();

            bool stringLiteralFound = false;
            StringBuilder stringBuilder = new StringBuilder();

            while (tokenStream.MoveNext())
            {
                var currentToken = tokenStream.Current;

                if (!stringLiteralFound)
                {
                    if (currentToken.TokenType == TokenType.QuoteSign)
                        stringLiteralFound = true;

                    yield return currentToken;
                }
                else
                {

                    if (currentToken.TokenType == TokenType.QuoteSign)
                    {
                        stringLiteralFound = false;
                        yield return new JsonToken() { TokenType = TokenType.String, Value = stringBuilder.ToString() };
                        stringBuilder.Clear();
                        yield return currentToken;
                    }
                    else
                        stringBuilder.Append(currentToken.Value);
                }

            }
        }

        public static IEnumerable<JsonToken> TokenizeJson(string json)
        {
            Dictionary<char, TokenType> tokenTypes = new Dictionary<char, TokenType>()
            {
                { '"', TokenType.QuoteSign },
                { ':', TokenType.Colon },
                { '{', TokenType.BracketOpen },
                { '}', TokenType.BracketClose },
                { ',', TokenType.Comma },
            };

            StringBuilder dataBuilder = new StringBuilder();

            for (int i = 0; i < json.Length; i++)
            {
                var c = json[i];

                if (tokenTypes.ContainsKey(c))
                {
                    if (dataBuilder.Length > 0)
                    {
                        yield return new JsonToken()
                        {
                            TokenType = TokenType.String,
                            Value = dataBuilder.ToString()
                        };

                        dataBuilder.Clear();
                    }
                    yield return new JsonToken()
                    {
                        TokenType = tokenTypes[c],
                        Value = c + ""
                    };
                }
                else
                {
                    dataBuilder.Append(c);
                }

            }
        }
    }
}
