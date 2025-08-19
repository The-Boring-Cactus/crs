using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Npgsql.Replication.PgOutput.Messages.RelationMessage;

namespace FunctEngine
{
    public enum TokenType
    {
        Identifier, Number, String, Operator, Keyword,
        LeftParen, RightParen, LeftBracket, RightBracket,
        LeftBrace, RightBrace,
        Comma, Semicolon, Equals, Dot, EOF,
        Plus, Minus, Star, Slash, Percent,
        EqualEqual, NotEqual, Less, LessEqual, Greater, GreaterEqual,
        And, Or, Not,
        True, False
    }

    public class Token
    {
        public TokenType Type { get; set; }
        public string Value { get; set; }
        public int Line { get; set; }
        public int Column { get; set; }
    }

    public class Tokenizer
    {
        private string input;
        private int position=0;
        private int line = 1;
        private int column = 1;

        public List<Token> Tokenize(string code)
        {
            this.input = code;

            var tokens = new List<Token>();

            while (position < input.Length)
            {
                SkipWhitespaceAndComments();
                if (position >= input.Length) break;

                var token = NextToken();
                if (token != null)
                    tokens.Add(token);
            }

            tokens.Add(new Token { Type = TokenType.EOF, Line = line, Column = column });
            return tokens;
        }
        private void SkipWhitespaceAndComments()
        {
            while (position < input.Length)
            {
                if (char.IsWhiteSpace(input[position]))
                {
                    if (input[position] == '\n')
                    {
                        line++;
                        column = 1;
                    }
                    else
                    {
                        column++;
                    }
                    position++;
                }
                else if (position < input.Length - 1 && input[position] == '/' && input[position + 1] == '/')
                {
                    // Skip single-line comment
                    while (position < input.Length && input[position] != '\n')
                        position++;
                }
                else
                {
                    break;
                }
            }
        }

        private Token NextToken()
        {
            if (position >= input.Length) return null;

            var startCol = column;
            var ch = input[position];

            // String literals
            if (ch == '\'' || ch == '"')
            {
                return ReadString(ch);
            }

            // Numbers
            if (char.IsDigit(ch))
            {
                return ReadNumber();
            }

            // Identifiers and keywords
            if (char.IsLetter(ch) || ch == '_')
            {
                return ReadIdentifier();
            }

            // Two-character operators
            if (position + 1 < input.Length)
            {
                var twoChar = input.Substring(position, 2);
                TokenType? twoCharType = null;

                switch (twoChar)
                {
                    case "==": twoCharType = TokenType.EqualEqual; break;
                    case "!=": twoCharType = TokenType.NotEqual; break;
                    case "<=": twoCharType = TokenType.LessEqual; break;
                    case ">=": twoCharType = TokenType.GreaterEqual; break;
                    case "&&": twoCharType = TokenType.And; break;
                    case "||": twoCharType = TokenType.Or; break;
                }

                if (twoCharType.HasValue)
                {
                    position += 2;
                    column += 2;
                    return new Token { Type = twoCharType.Value, Value = twoChar, Line = line, Column = startCol };
                }
            }

            // Single character tokens
            position++;
            column++;

            switch (ch)
            {
                case '(': return new Token { Type = TokenType.LeftParen, Value = "(", Line = line, Column = startCol };
                case ')': return new Token { Type = TokenType.RightParen, Value = ")", Line = line, Column = startCol };
                case '[': return new Token { Type = TokenType.LeftBracket, Value = "[", Line = line, Column = startCol };
                case ']': return new Token { Type = TokenType.RightBracket, Value = "]", Line = line, Column = startCol };
                case '{': return new Token { Type = TokenType.LeftBrace, Value = "{", Line = line, Column = startCol };
                case '}': return new Token { Type = TokenType.RightBrace, Value = "}", Line = line, Column = startCol };
                case ',': return new Token { Type = TokenType.Comma, Value = ",", Line = line, Column = startCol };
                case ';': return new Token { Type = TokenType.Semicolon, Value = ";", Line = line, Column = startCol };
                case '=': return new Token { Type = TokenType.Equals, Value = "=", Line = line, Column = startCol };
                case '.': return new Token { Type = TokenType.Dot, Value = ".", Line = line, Column = startCol };
                case '+': return new Token { Type = TokenType.Plus, Value = "+", Line = line, Column = startCol };
                case '-': return new Token { Type = TokenType.Minus, Value = "-", Line = line, Column = startCol };
                case '*': return new Token { Type = TokenType.Star, Value = "*", Line = line, Column = startCol };
                case '/': return new Token { Type = TokenType.Slash, Value = "/", Line = line, Column = startCol };
                case '%': return new Token { Type = TokenType.Percent, Value = "%", Line = line, Column = startCol };
                case '<': return new Token { Type = TokenType.Less, Value = "<", Line = line, Column = startCol };
                case '>': return new Token { Type = TokenType.Greater, Value = ">", Line = line, Column = startCol };
                case '!': return new Token { Type = TokenType.Not, Value = "!", Line = line, Column = startCol };
            }

            throw new Exception($"Unexpected character '{ch}' at line {line}, column {startCol}");
        }
        private Token ReadString(char quote)
        {
            var sb = new StringBuilder();
            var startCol = column;
            position++; // Skip opening quote
            column++;

            while (position < input.Length && input[position] != quote)
            {
                if (input[position] == '\\' && position + 1 < input.Length)
                {
                    position++;
                    column++;
                    switch (input[position])
                    {
                        case 'n': sb.Append('\n'); break;
                        case 't': sb.Append('\t'); break;
                        case 'r': sb.Append('\r'); break;
                        case '\\': sb.Append('\\'); break;
                        case '\'': sb.Append('\''); break;
                        case '"': sb.Append('"'); break;
                        default: sb.Append(input[position]); break;
                    }
                }
                else
                {
                    sb.Append(input[position]);
                }
                position++;
                column++;
            }

            if (position >= input.Length)
                throw new Exception($"Unterminated string at line {line}, column {startCol}");

            position++; // Skip closing quote
            column++;

            return new Token { Type = TokenType.String, Value = sb.ToString(), Line = line, Column = startCol };
        }

        private Token ReadNumber()
        {
            var sb = new StringBuilder();
            var startCol = column;

            while (position < input.Length && (char.IsDigit(input[position]) || input[position] == '.'))
            {
                sb.Append(input[position]);
                position++;
                column++;
            }

            return new Token { Type = TokenType.Number, Value = sb.ToString(), Line = line, Column = startCol };
        }


        private Token ReadIdentifier()
        {
            var sb = new StringBuilder();
            var startCol = column;

            while (position < input.Length && (char.IsLetterOrDigit(input[position]) || input[position] == '_'))
            {
                sb.Append(input[position]);
                position++;
                column++;
            }

            var value = sb.ToString();

            // Check for boolean literals
            if (value == "true")
                return new Token { Type = TokenType.True, Value = value, Line = line, Column = startCol };
            if (value == "false")
                return new Token { Type = TokenType.False, Value = value, Line = line, Column = startCol };

            var type = IsKeyword(value) ? TokenType.Keyword : TokenType.Identifier;

            return new Token { Type = type, Value = value, Line = line, Column = startCol };
        }

        private bool IsKeyword(string value)
        {
            return value == "var" || value == "if" || value == "else" || value == "while" ||
                   value == "for" || value == "function" || value == "return" || value == "null";
        }
    }
}
