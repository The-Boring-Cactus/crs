using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FunctEngine
{
    public enum TokenType
    {
        Number, String, Identifier, Operator, Delimiter, Whitespace, Comment, EOF
    }

    public class Token
    {
        public TokenType Type { get; set; }
        public string Value { get; set; }
        public int Position { get; set; }
    }

    public class Tokenizer
    {
        private readonly Dictionary<TokenType, string> patterns = new Dictionary<TokenType, string>
        {
            [TokenType.Number] = @"\d+(\.\d+)?",
            [TokenType.String] = @"'([^'\\]|\\.)*'",
            [TokenType.Identifier] = @"[a-zA-Z_][a-zA-Z0-9_]*",
            [TokenType.Operator] = @"(\+\+|--|==|!=|<=|>=|&&|\|\||[+\-*/=<>!])",
            [TokenType.Delimiter] = @"[(){}[\];,]",
            [TokenType.Whitespace] = @"\s+",
            [TokenType.Comment] = @"//[^\r\n]*"
        };

        public List<Token> Tokenize(string code)
        {
            var tokens = new List<Token>();
            string input = code;
            int position = 0;

            while (position < input.Length)
            {
                bool matched = false;

                foreach (var pattern in patterns)
                {
                    var regex = new Regex($"^{pattern.Value}");
                    var match = regex.Match(input.Substring(position));

                    if (match.Success)
                    {
                        if (pattern.Key != TokenType.Whitespace && pattern.Key != TokenType.Comment)
                        {
                            tokens.Add(new Token
                            {
                                Type = pattern.Key,
                                Value = match.Value,
                                Position = position
                            });
                        }
                        position += match.Length;
                        matched = true;
                        break;
                    }
                }

                if (!matched)
                {
                    position++;
                }
            }

            return tokens;
        }
    }
}
