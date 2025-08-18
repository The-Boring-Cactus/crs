using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FunctEngine
{
    public class Parser
    {
        private readonly List<Token> tokens;
        private int current = 0;

        public Parser(List<Token> tokens)
        {
            this.tokens = tokens;
        }

        public bool IsAtEnd() => current >= tokens.Count;

        private Token Peek() => IsAtEnd() ? new Token { Type = TokenType.EOF } : tokens[current];
        private Token Previous() => tokens[current - 1];
        private Token Advance() => IsAtEnd() ? Peek() : tokens[current++];

        private bool Check(TokenType type) => !IsAtEnd() && Peek().Type == type;
        private bool Match(params TokenType[] types) => types.Any(Check) && (Advance() != null);

        public List<Statement> Parse()
        {
            var statements = new List<Statement>();

            while (!IsAtEnd())
            {
                var statement = ParseStatement();
                if (statement != null)
                    statements.Add(statement);
            }

            return statements;
        }

        public Statement ParseStatement()
        {
            try
            {
                if (Match(TokenType.Identifier) && Previous().Value == "var")
                    return ParseVariableDeclaration();
                if (Match(TokenType.Identifier) && Previous().Value == "for")
                    return ParseForStatement();
                if (Match(TokenType.Identifier) && Previous().Value == "while")
                    return ParseWhileStatement();
                return ParseExpressionStatement();
            }
            catch
            {
                // Sincronizar en caso de error
                Synchronize();
                return null;
            }
        }

        private VariableDeclaration ParseVariableDeclaration()
        {
            if (!Check(TokenType.Identifier))
                throw new Exception("Expected variable name");

            string name = Advance().Value;
            Expression initializer = null;

            if (Match(TokenType.Operator) && Previous().Value == "=")
            {
                initializer = ParseExpression();
            }

            Consume(TokenType.Delimiter, ";", "Expected ';' after variable declaration");
            return new VariableDeclaration { Name = name, Initializer = initializer };
        }

        private ForStatement ParseForStatement()
        {
            Consume(TokenType.Delimiter, "(", "Expected '(' after 'for'");

            Statement initializer = null;
            if (!Check(TokenType.Delimiter) || Peek().Value != ";")
                initializer = ParseStatement();
            else
                Advance(); // consume ';'

            Expression condition = null;
            if (!Check(TokenType.Delimiter) || Peek().Value != ";")
                condition = ParseExpression();
            Consume(TokenType.Delimiter, ";", "Expected ';' after for condition");

            Expression increment = null;
            if (!Check(TokenType.Delimiter) || Peek().Value != ")")
                increment = ParseExpression();
            Consume(TokenType.Delimiter, ")", "Expected ')' after for clauses");

            Consume(TokenType.Delimiter, "{", "Expected '{' before for body");
            var body = new List<Statement>();
            while (!Check(TokenType.Delimiter) || Peek().Value != "}")
            {
                if (IsAtEnd()) break;
                var stmt = ParseStatement();
                if (stmt != null) body.Add(stmt);
            }
            Consume(TokenType.Delimiter, "}", "Expected '}' after for body");

            return new ForStatement
            {
                Initializer = initializer,
                Condition = condition,
                Increment = increment,
                Body = body
            };
        }

        private WhileStatement ParseWhileStatement()
        {
            Consume(TokenType.Delimiter, "(", "Expected '(' after 'while'");

            Expression condition = ParseExpression();
            Consume(TokenType.Delimiter, ")", "Expected ')' after while condition");

            Consume(TokenType.Delimiter, "{", "Expected '{' before while body");
            var body = new List<Statement>();
            while (!Check(TokenType.Delimiter) || Peek().Value != "}")
            {
                if (IsAtEnd()) break;
                var stmt = ParseStatement();
                if (stmt != null) body.Add(stmt);
            }
            Consume(TokenType.Delimiter, "}", "Expected '}' after while body");

            return new WhileStatement
            {
                Condition = condition,
                Body = body
            };
        }

        private ExpressionStatement ParseExpressionStatement()
        {
            var expr = ParseExpression();
            if (Check(TokenType.Delimiter) && Peek().Value == ";")
                Advance();
            return new ExpressionStatement { Expression = expr };
        }

        private Expression ParseExpression()
        {
            return ParseAssignment();
        }

        private Expression ParseAssignment()
        {
            var expr = ParseOr();

            if (Match(TokenType.Operator) && Previous().Value == "=")
            {
                var value = ParseAssignment();
                if (expr is IdentifierExpression identifier)
                {
                    return new AssignmentExpression { Variable = identifier.Name, Value = value };
                }
                throw new Exception("Invalid assignment target");
            }

            return expr;
        }

        private Expression ParseOr()
        {
            var expr = ParseAnd();

            while (Match(TokenType.Operator) && Previous().Value == "||")
            {
                string op = Previous().Value;
                var right = ParseAnd();
                expr = new BinaryExpression { Left = expr, Operator = op, Right = right };
            }

            return expr;
        }

        private Expression ParseAnd()
        {
            var expr = ParseEquality();

            while (Match(TokenType.Operator) && Previous().Value == "&&")
            {
                string op = Previous().Value;
                var right = ParseEquality();
                expr = new BinaryExpression { Left = expr, Operator = op, Right = right };
            }

            return expr;
        }

        private Expression ParseEquality()
        {
            var expr = ParseComparison();

            while (Match(TokenType.Operator) && (Previous().Value == "==" || Previous().Value == "!="))
            {
                string op = Previous().Value;
                var right = ParseComparison();
                expr = new BinaryExpression { Left = expr, Operator = op, Right = right };
            }

            return expr;
        }

        private Expression ParseComparison()
        {
            var expr = ParseTerm();

            while (Match(TokenType.Operator) && (Previous().Value == ">" || Previous().Value == ">=" ||
                   Previous().Value == "<" || Previous().Value == "<="))
            {
                string op = Previous().Value;
                var right = ParseTerm();
                expr = new BinaryExpression { Left = expr, Operator = op, Right = right };
            }

            return expr;
        }

        private Expression ParseTerm()
        {
            var expr = ParseFactor();

            while (Match(TokenType.Operator) && (Previous().Value == "-" || Previous().Value == "+"))
            {
                string op = Previous().Value;
                var right = ParseFactor();
                expr = new BinaryExpression { Left = expr, Operator = op, Right = right };
            }

            return expr;
        }

        private Expression ParseFactor()
        {
            var expr = ParseUnary();

            while (Match(TokenType.Operator) && (Previous().Value == "/" || Previous().Value == "*"))
            {
                string op = Previous().Value;
                var right = ParseUnary();
                expr = new BinaryExpression { Left = expr, Operator = op, Right = right };
            }

            return expr;
        }

        private Expression ParseUnary()
        {
            if (Match(TokenType.Operator) && (Previous().Value == "!" || Previous().Value == "-"))
            {
                string op = Previous().Value;
                var right = ParseUnary();
                return new BinaryExpression { Left = new LiteralExpression { Value = 0 }, Operator = op, Right = right };
            }

            return ParseCall();
        }

        private Expression ParseCall()
        {
            var expr = ParsePrimary();

            while (true)
            {
                if (Match(TokenType.Delimiter) && Previous().Value == "(")
                {
                    expr = FinishCall(expr);
                }
                else
                {
                    break;
                }
            }

            return expr;
        }

        private Expression FinishCall(Expression callee)
        {
            var arguments = new List<Expression>();

            if (!Check(TokenType.Delimiter) || Peek().Value != ")")
            {
                do
                {
                    arguments.Add(ParseExpression());
                } while (Match(TokenType.Delimiter) && Previous().Value == ",");
            }

            Consume(TokenType.Delimiter, ")", "Expected ')' after arguments");

            if (callee is IdentifierExpression identifier)
            {
                return new CallExpression { FunctionName = identifier.Name, Arguments = arguments };
            }

            throw new Exception("Invalid function call");
        }

        private Expression ParsePrimary()
        {
            if (Match(TokenType.Number))
            {
                return new LiteralExpression { Value = Convert.ToDouble(Previous().Value, CultureInfo.InvariantCulture) };
            }

            if (Match(TokenType.String))
            {
                string value = Previous().Value;
                // Remover las comillas simples
                value = value.Substring(1, value.Length - 2);
                return new LiteralExpression { Value = value };
            }

            if (Match(TokenType.Identifier))
            {
                var identifier = new IdentifierExpression { Name = Previous().Value };

                // Verificar si es acceso a array
                if (Match(TokenType.Delimiter) && Previous().Value == "[")
                {
                    var index = ParseExpression();
                    Consume(TokenType.Delimiter, "]", "Expected ']' after array index");
                    return new ArrayAccessExpression { Array = identifier, Index = index };
                }

                return identifier;
            }

            if (Match(TokenType.Delimiter) && Previous().Value == "(")
            {
                var expr = ParseExpression();
                Consume(TokenType.Delimiter, ")", "Expected ')' after expression");
                return expr;
            }

            // Parse array literal [1, 2, 3]
            if (Match(TokenType.Delimiter) && Previous().Value == "[")
            {
                var elements = new List<Expression>();

                if (!Check(TokenType.Delimiter) || Peek().Value != "]")
                {
                    do
                    {
                        elements.Add(ParseExpression());
                    } while (Match(TokenType.Delimiter) && Previous().Value == ",");
                }

                Consume(TokenType.Delimiter, "]", "Expected ']' after array elements");
                return new ArrayExpression { Elements = elements };
            }

            throw new Exception($"Unexpected token: {Peek().Value}");
        }

        private void Consume(TokenType type, string value, string message)
        {
            if (Check(type) && Peek().Value == value)
            {
                Advance();
                return;
            }

            throw new Exception(message);
        }

        private void Synchronize()
        {
            Advance();

            while (!IsAtEnd())
            {
                if (Previous().Value == ";") return;

                switch (Peek().Value)
                {
                    case "class":
                    case "fun":
                    case "var":
                    case "for":
                    case "if":
                    case "while":
                    case "return":
                        return;
                }

                Advance();
            }
        }
    }
}
