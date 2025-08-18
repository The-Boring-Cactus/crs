using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FunctEngine
{
    public class ExpressionEvaluator
    {
        private readonly Dictionary<string, object> variables;
        private readonly FunctionManager functionManager;

        public ExpressionEvaluator(Dictionary<string, object> variables, FunctionManager functionManager)
        {
            this.variables = variables;
            this.functionManager = functionManager;
        }

        public object Evaluate(Expression expression)
        {
            return expression switch
            {
                LiteralExpression literal => literal.Value,
                IdentifierExpression identifier => GetVariableValue(identifier.Name),
                BinaryExpression binary => EvaluateBinary(binary),
                CallExpression call => EvaluateCall(call),
                AssignmentExpression assignment => EvaluateAssignment(assignment),
                ArrayExpression array => EvaluateArray(array),
                ArrayAccessExpression arrayAccess => EvaluateArrayAccess(arrayAccess),
                _ => null
            };
        }

        private object GetVariableValue(string name)
        {
            return variables.ContainsKey(name) ? variables[name] : 0;
        }

        private object EvaluateBinary(BinaryExpression binary)
        {
            var left = Evaluate(binary.Left);

            // Evaluación cortocircuito para operadores lógicos
            if (binary.Operator == "&&")
            {
                if (!Convert.ToBoolean(left)) return false;
                var rightv = Evaluate(binary.Right);
                return Convert.ToBoolean(rightv);
            }

            if (binary.Operator == "||")
            {
                if (Convert.ToBoolean(left)) return true;
                var rightv = Evaluate(binary.Right);
                return Convert.ToBoolean(rightv);
            }

            var right = Evaluate(binary.Right);

            return binary.Operator switch
            {
                "+" => Convert.ToDouble(left) + Convert.ToDouble(right),
                "-" => Convert.ToDouble(left) - Convert.ToDouble(right),
                "*" => Convert.ToDouble(left) * Convert.ToDouble(right),
                "/" => Convert.ToDouble(left) / Convert.ToDouble(right),
                "%" => Convert.ToDouble(left) % Convert.ToDouble(right),
                "<" => Convert.ToDouble(left) < Convert.ToDouble(right),
                ">" => Convert.ToDouble(left) > Convert.ToDouble(right),
                "<=" => Convert.ToDouble(left) <= Convert.ToDouble(right),
                ">=" => Convert.ToDouble(left) >= Convert.ToDouble(right),
                "==" => AreEqual(left, right),
                "!=" => !AreEqual(left, right),
                "!" => !Convert.ToBoolean(right),
                _ => throw new Exception($"Operador desconocido: {binary.Operator}")
            };
        }

        private bool AreEqual(object left, object right)
        {
            if (left == null && right == null) return true;
            if (left == null || right == null) return false;

            // Comparación numérica
            if (IsNumeric(left) && IsNumeric(right))
            {
                return Convert.ToDouble(left) == Convert.ToDouble(right);
            }

            // Comparación de strings
            return left.ToString() == right.ToString();
        }

        private bool IsNumeric(object value)
        {
            return value is int || value is double || value is float || value is decimal ||
                   double.TryParse(value?.ToString(), out _);
        }

        private object EvaluateCall(CallExpression call)
        {
            var args = call.Arguments.Select(Evaluate).ToArray();
            return functionManager.CallFunction(call.FunctionName, args);
        }

        private object EvaluateAssignment(AssignmentExpression assignment)
        {
            var value = Evaluate(assignment.Value);
            variables[assignment.Variable] = value;
            return value;
        }

        private object EvaluateArray(ArrayExpression array)
        {
            return new List<object>(array.Elements.Select(Evaluate));
        }

        private object EvaluateArrayAccess(ArrayAccessExpression arrayAccess)
        {
            var arrayObj = Evaluate(arrayAccess.Array);
            var indexObj = Evaluate(arrayAccess.Index);

            if (arrayObj is List<object> list && indexObj != null)
            {
                int index = Convert.ToInt32(indexObj);
                if (index >= 0 && index < list.Count)
                {
                    return list[index];
                }
            }

            return null;
        }
    }
}
