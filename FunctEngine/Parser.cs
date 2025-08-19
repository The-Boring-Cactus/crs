using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace FunctEngine
{
    public class Parser
    {
        private List<Token> tokens;
        private int position;

        public Parser(List<Token> tokens)
        {
            this.tokens = tokens;
            this.position = 0;
        }

        public ProgramNode Parse()
        {
            var program = new ProgramNode();

            while (!IsAtEnd())
            {
                var stmt = ParseStatement();
                if (stmt != null)
                    program.Statements.Add(stmt);
            }

            return program;
        }

        private ASTNode ParseStatement()
        {
            if (Match(TokenType.Keyword, "var"))
            {
                return ParseVarDeclaration();
            }

            if (Match(TokenType.Keyword, "if"))
            {
                return ParseIfStatement();
            }

            if (Match(TokenType.Keyword, "while"))
            {
                return ParseWhileStatement();
            }

            if (Match(TokenType.LeftBrace))
            {
                return ParseBlock();
            }

            if (Peek().Type == TokenType.Identifier)
            {
                var next = PeekNext();
                if (next != null && next.Type == TokenType.Equals)
                {
                    return ParseAssignment();
                }
            }

            var expression = ParseExpression();
            Consume(TokenType.Semicolon, "Expected ';' after expression");
            return expression;
        }

        private VarDeclarationNode ParseVarDeclaration()
        {
            var name = Consume(TokenType.Identifier, "Expected variable name").Value;
            Consume(TokenType.Equals, "Expected '=' in variable declaration");
            var value = ParseExpression();
            Consume(TokenType.Semicolon, "Expected ';' after variable declaration");

            return new VarDeclarationNode { Name = name, Value = value };
        }

        private AssignmentNode ParseAssignment()
        {
            var name = Consume(TokenType.Identifier, "Expected variable name").Value;
            Consume(TokenType.Equals, "Expected '=' in assignment");
            var value = ParseExpression();
            Consume(TokenType.Semicolon, "Expected ';' after assignment");

            return new AssignmentNode { Name = name, Value = value };
        }

        private IfNode ParseIfStatement()
        {
            Consume(TokenType.LeftParen, "Expected '(' after 'if'");
            var condition = ParseExpression();
            Consume(TokenType.RightParen, "Expected ')' after if condition");

            var thenBranch = ParseStatement();
            ASTNode elseBranch = null;

            if (Match(TokenType.Keyword, "else"))
            {
                elseBranch = ParseStatement();
            }

            return new IfNode { Condition = condition, ThenBranch = thenBranch, ElseBranch = elseBranch };
        }

        private WhileNode ParseWhileStatement()
        {
            Consume(TokenType.LeftParen, "Expected '(' after 'while'");
            var condition = ParseExpression();
            Consume(TokenType.RightParen, "Expected ')' after while condition");

            var body = ParseStatement();

            return new WhileNode { Condition = condition, Body = body };
        }

        private BlockNode ParseBlock()
        {
            var block = new BlockNode();

            while (!Check(TokenType.RightBrace) && !IsAtEnd())
            {
                block.Statements.Add(ParseStatement());
            }

            Consume(TokenType.RightBrace, "Expected '}' after block");
            return block;
        }

        private ASTNode ParseExpression()
        {
            return ParseLogicalOr();
        }

        private ASTNode ParseLogicalOr()
        {
            var expr = ParseLogicalAnd();

            while (Match(TokenType.Or))
            {
                var op = Previous().Value;
                var right = ParseLogicalAnd();
                expr = new BinaryOpNode { Left = expr, Operator = op, Right = right };
            }

            return expr;
        }

        private ASTNode ParseLogicalAnd()
        {
            var expr = ParseEquality();

            while (Match(TokenType.And))
            {
                var op = Previous().Value;
                var right = ParseEquality();
                expr = new BinaryOpNode { Left = expr, Operator = op, Right = right };
            }

            return expr;
        }

        private ASTNode ParseEquality()
        {
            var expr = ParseRelational();

            while (Match(TokenType.EqualEqual) || Match(TokenType.NotEqual))
            {
                var op = Previous().Value;
                var right = ParseRelational();
                expr = new BinaryOpNode { Left = expr, Operator = op, Right = right };
            }

            return expr;
        }

        private ASTNode ParseRelational()
        {
            var expr = ParseAdditive();

            while (Match(TokenType.Less) || Match(TokenType.LessEqual) ||
                   Match(TokenType.Greater) || Match(TokenType.GreaterEqual))
            {
                var op = Previous().Value;
                var right = ParseAdditive();
                expr = new BinaryOpNode { Left = expr, Operator = op, Right = right };
            }

            return expr;
        }

        private ASTNode ParseAdditive()
        {
            var expr = ParseMultiplicative();

            while (Match(TokenType.Plus) || Match(TokenType.Minus))
            {
                var op = Previous().Value;
                var right = ParseMultiplicative();
                expr = new BinaryOpNode { Left = expr, Operator = op, Right = right };
            }

            return expr;
        }

        private ASTNode ParseMultiplicative()
        {
            var expr = ParseUnary();

            while (Match(TokenType.Star) || Match(TokenType.Slash) || Match(TokenType.Percent))
            {
                var op = Previous().Value;
                var right = ParseUnary();
                expr = new BinaryOpNode { Left = expr, Operator = op, Right = right };
            }

            return expr;
        }

        private ASTNode ParseUnary()
        {
            if (Match(TokenType.Not) || Match(TokenType.Minus))
            {
                var op = Previous().Value;
                var expr = ParseUnary();
                return new UnaryOpNode { Operator = op, Operand = expr };
            }

            return ParsePrimary();
        }

        private ASTNode ParsePrimary()
        {
            if (Match(TokenType.True))
            {
                return new BooleanNode { Value = true };
            }

            if (Match(TokenType.False))
            {
                return new BooleanNode { Value = false };
            }

            if (Match(TokenType.Number))
            {
                return new NumberNode { Value = double.Parse(Previous().Value) };
            }

            if (Match(TokenType.String))
            {
                return new StringNode { Value = Previous().Value };
            }

            if (Match(TokenType.LeftParen))
            {
                var expr = ParseExpression();
                Consume(TokenType.RightParen, "Expected ')' after expression");
                return expr;
            }

            if (Match(TokenType.Identifier))
            {
                var name = Previous().Value;

                if (Match(TokenType.LeftParen))
                {
                    return ParseFunctionCall(name);
                }
                else if (Match(TokenType.LeftBracket))
                {
                    var index = ParseExpression();
                    Consume(TokenType.RightBracket, "Expected ']' after array index");
                    return new ArrayAccessNode { ArrayName = name, Index = index };
                }

                return new IdentifierNode { Name = name };
            }

            throw new Exception($"Unexpected token: {Peek().Value} at line {Peek().Line}");
        }

        private FunctionCallNode ParseFunctionCall(string name)
        {
            var node = new FunctionCallNode { Name = name };

            if (!Check(TokenType.RightParen))
            {
                do
                {
                    node.Arguments.Add(ParseExpression());
                } while (Match(TokenType.Comma));
            }

            Consume(TokenType.RightParen, "Expected ')' after function arguments");
            return node;
        }

        private bool Match(TokenType type, string value = null)
        {
            if (Check(type) && (value == null || Peek().Value == value))
            {
                Advance();
                return true;
            }
            return false;
        }

        private bool Check(TokenType type)
        {
            if (IsAtEnd()) return false;
            return Peek().Type == type;
        }

        private Token Advance()
        {
            if (!IsAtEnd()) position++;
            return Previous();
        }

        private bool IsAtEnd()
        {
            return Peek().Type == TokenType.EOF;
        }

        private Token Peek()
        {
            return tokens[position];
        }

        private Token PeekNext()
        {
            if (position + 1 < tokens.Count)
                return tokens[position + 1];
            return null;
        }

        private Token Previous()
        {
            return tokens[position - 1];
        }

        private Token Consume(TokenType type, string message)
        {
            if (Check(type)) return Advance();
            throw new Exception($"{message} at line {Peek().Line}, column {Peek().Column}");
        }
    }
}
