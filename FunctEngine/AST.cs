using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FunctEngine
{
    public abstract class ASTNode { }

    public class ProgramNode : ASTNode
    {
        public List<ASTNode> Statements { get; set; } = new List<ASTNode>();
    }

    public class VarDeclarationNode : ASTNode
    {
        public string Name { get; set; }
        public ASTNode Value { get; set; }
    }

    public class AssignmentNode : ASTNode
    {
        public string Name { get; set; }
        public ASTNode Value { get; set; }
    }

    public class FunctionCallNode : ASTNode
    {
        public string Name { get; set; }
        public List<ASTNode> Arguments { get; set; } = new List<ASTNode>();
    }

    public class NumberNode : ASTNode
    {
        public double Value { get; set; }
    }

    public class StringNode : ASTNode
    {
        public string Value { get; set; }
    }

    public class BooleanNode : ASTNode
    {
        public bool Value { get; set; }
    }

    public class IdentifierNode : ASTNode
    {
        public string Name { get; set; }
    }

    public class ArrayAccessNode : ASTNode
    {
        public string ArrayName { get; set; }
        public ASTNode Index { get; set; }
    }

    public class BinaryOpNode : ASTNode
    {
        public ASTNode Left { get; set; }
        public string Operator { get; set; }
        public ASTNode Right { get; set; }
    }

    public class UnaryOpNode : ASTNode
    {
        public string Operator { get; set; }
        public ASTNode Operand { get; set; }
    }

    public class IfNode : ASTNode
    {
        public ASTNode Condition { get; set; }
        public ASTNode ThenBranch { get; set; }
        public ASTNode ElseBranch { get; set; }
    }

    public class WhileNode : ASTNode
    {
        public ASTNode Condition { get; set; }
        public ASTNode Body { get; set; }
    }

    public class BlockNode : ASTNode
    {
        public List<ASTNode> Statements { get; set; } = new List<ASTNode>();
    }
}
