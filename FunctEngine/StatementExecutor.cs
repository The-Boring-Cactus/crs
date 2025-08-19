using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace FunctEngine
{
    public class StatementExecutor
    {
        private readonly Dictionary<string, object> variables;
        private readonly FunctionManager functionManager;

        public StatementExecutor(Dictionary<string, object> variables, FunctionManager functionManager)
        {
            this.variables = variables;
            this.functionManager = functionManager;
        }

        public void ExecuteStatements(List<ASTNode> statements)
        {
            foreach (var statement in statements)
            {
                Evaluate(statement);
            }
        }

        public object Evaluate(ASTNode node)
        {
            switch (node)
            {
                case VarDeclarationNode varDecl:
                    var value = Evaluate(varDecl.Value);
                    variables[varDecl.Name] = value;
                    return value;

                case AssignmentNode assignment:
                    var assignValue = Evaluate(assignment.Value);
                    variables[assignment.Name] = assignValue;
                    return assignValue;

                case FunctionCallNode funcCall:
                    var args = funcCall.Arguments.Select(arg => Evaluate(arg)).ToArray();
                    return functionManager.CallFunction(funcCall.Name, args);

                case NumberNode number:
                    return number.Value;

                case StringNode str:
                    return str.Value;

                case BooleanNode boolean:
                    return boolean.Value;

                case IdentifierNode identifier:
                    return variables[identifier.Name];

                case ArrayAccessNode arrayAccess:
                    var array = variables[arrayAccess.ArrayName] as object[];
                    var index = Convert.ToInt32(Evaluate(arrayAccess.Index));
                    return array[index];

                case BinaryOpNode binaryOp:
                    return EvaluateBinaryOp(binaryOp);

                case UnaryOpNode unaryOp:
                    return EvaluateUnaryOp(unaryOp);

                case IfNode ifNode:
                    var condition = ConvertToBool(Evaluate(ifNode.Condition));
                    if (condition)
                        return Evaluate(ifNode.ThenBranch);
                    else if (ifNode.ElseBranch != null)
                        return Evaluate(ifNode.ElseBranch);
                    return null;

                case WhileNode whileNode:
                    while (ConvertToBool(Evaluate(whileNode.Condition)))
                    {
                        Evaluate(whileNode.Body);
                    }
                    return null;

                case BlockNode block:
                    foreach (var stmt in block.Statements)
                    {
                        Evaluate(stmt);
                    }
                    return null;

                default:
                    throw new Exception($"Unknown node type: {node.GetType().Name}");
            }
        }
        private object EvaluateBinaryOp(BinaryOpNode node)
        {
            var left = Evaluate(node.Left);
            var right = Evaluate(node.Right);

            switch (node.Operator)
            {
                case "+":
                    if (left is string || right is string)
                        return left.ToString() + right.ToString();
                    return Convert.ToDouble(left) + Convert.ToDouble(right);

                case "-":
                    return Convert.ToDouble(left) - Convert.ToDouble(right);

                case "*":
                    return Convert.ToDouble(left) * Convert.ToDouble(right);

                case "/":
                    return Convert.ToDouble(left) / Convert.ToDouble(right);

                case "%":
                    return Convert.ToDouble(left) % Convert.ToDouble(right);

                case "==":
                    return AreEqual(left, right);

                case "!=":
                    return !AreEqual(left, right);

                case "<":
                    return Convert.ToDouble(left) < Convert.ToDouble(right);

                case "<=":
                    return Convert.ToDouble(left) <= Convert.ToDouble(right);

                case ">":
                    return Convert.ToDouble(left) > Convert.ToDouble(right);

                case ">=":
                    return Convert.ToDouble(left) >= Convert.ToDouble(right);

                case "&&":
                    return ConvertToBool(left) && ConvertToBool(right);

                case "||":
                    return ConvertToBool(left) || ConvertToBool(right);

                default:
                    throw new Exception($"Unknown binary operator: {node.Operator}");
            }
        }

        private object EvaluateUnaryOp(UnaryOpNode node)
        {
            var operand = Evaluate(node.Operand);

            switch (node.Operator)
            {
                case "!":
                    return !ConvertToBool(operand);

                case "-":
                    return -Convert.ToDouble(operand);

                default:
                    throw new Exception($"Unknown unary operator: {node.Operator}");
            }
        }

        private bool AreEqual(object left, object right)
        {
            if (left == null && right == null) return true;
            if (left == null || right == null) return false;

            if (left is bool && right is bool)
                return (bool)left == (bool)right;

            if (left is string && right is string)
                return (string)left == (string)right;

            if (IsNumeric(left) && IsNumeric(right))
                return Convert.ToDouble(left) == Convert.ToDouble(right);

            return left.Equals(right);
        }

        private bool IsNumeric(object value)
        {
            return value is double || value is int || value is float || value is decimal;
        }

        private bool ConvertToBool(object value)
        {
            if (value is bool) return (bool)value;
            if (value is double) return (double)value != 0;
            if (value is int) return (int)value != 0;
            if (value is string) return !string.IsNullOrEmpty((string)value);
            if (value == null) return false;
            return true;
        }
    }
}
