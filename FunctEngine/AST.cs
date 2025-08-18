using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FunctEngine
{
    public abstract class Statement { }
    public abstract class Expression { }

    // Statements
    public class ExpressionStatement : Statement
    {
        public Expression Expression { get; set; }
    }

    public class VariableDeclaration : Statement
    {
        public string Name { get; set; }
        public Expression Initializer { get; set; }
    }

    public class ForStatement : Statement
    {
        public Statement Initializer { get; set; }
        public Expression Condition { get; set; }
        public Expression Increment { get; set; }
        public List<Statement> Body { get; set; } = new List<Statement>();
    }

    public class WhileStatement : Statement
    {
        public Expression Condition { get; set; }
        public List<Statement> Body { get; set; } = new List<Statement>();
    }

    // Expressions
    public class LiteralExpression : Expression
    {
        public object Value { get; set; }
    }

    public class IdentifierExpression : Expression
    {
        public string Name { get; set; }
    }

    public class BinaryExpression : Expression
    {
        public Expression Left { get; set; }
        public string Operator { get; set; }
        public Expression Right { get; set; }
    }

    public class CallExpression : Expression
    {
        public string FunctionName { get; set; }
        public List<Expression> Arguments { get; set; } = new List<Expression>();
    }

    public class AssignmentExpression : Expression
    {
        public string Variable { get; set; }
        public Expression Value { get; set; }
    }

    public class ArrayExpression : Expression
    {
        public List<Expression> Elements { get; set; } = new List<Expression>();
    }

    public class ArrayAccessExpression : Expression
    {
        public Expression Array { get; set; }
        public Expression Index { get; set; }
    }
}
