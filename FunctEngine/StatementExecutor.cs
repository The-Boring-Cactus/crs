using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public void ExecuteStatements(List<Statement> statements)
        {
            foreach (var statement in statements)
            {
                ExecuteStatement(statement);
            }
        }

        private void ExecuteStatement(Statement statement)
        {
            switch (statement)
            {
                case ExpressionStatement expr:
                    EvaluateExpression(expr.Expression);
                    break;
                case VariableDeclaration varDecl:
                    var value = EvaluateExpression(varDecl.Initializer);
                    variables[varDecl.Name] = value;
                    break;
                case ForStatement forStmt:
                    ExecuteForLoop(forStmt);
                    break;
                case WhileStatement whileStmt:
                    ExecuteWhileLoop(whileStmt);
                    break;
            }
        }

        private void ExecuteForLoop(ForStatement forStmt)
        {
            // Ejecutar inicialización
            if (forStmt.Initializer != null)
                ExecuteStatement(forStmt.Initializer);

            // Ejecutar ciclo
            while (true)
            {
                // Evaluar condición
                if (forStmt.Condition != null)
                {
                    var conditionResult = EvaluateExpression(forStmt.Condition);
                    if (!Convert.ToBoolean(conditionResult))
                        break;
                }

                // Ejecutar cuerpo
                ExecuteStatements(forStmt.Body);

                // Ejecutar incremento
                if (forStmt.Increment != null)
                    EvaluateExpression(forStmt.Increment);
            }
        }

        private void ExecuteWhileLoop(WhileStatement whileStmt)
        {
            while (true)
            {
                // Evaluar condición
                var conditionResult = EvaluateExpression(whileStmt.Condition);
                if (!Convert.ToBoolean(conditionResult))
                    break;

                // Ejecutar cuerpo
                ExecuteStatements(whileStmt.Body);
            }
        }

        private object EvaluateExpression(Expression expression)
        {
            switch (expression)
            {
                case LiteralExpression literal:
                    return literal.Value;
                case IdentifierExpression identifier:
                    return variables.ContainsKey(identifier.Name) ? variables[identifier.Name] : 0;
                case BinaryExpression binary:
                    return EvaluateBinaryExpression(binary);
                case CallExpression call:
                    return EvaluateFunctionCall(call);
                case AssignmentExpression assignment:
                    var value = EvaluateExpression(assignment.Value);
                    variables[assignment.Variable] = value;
                    return value;
                case ArrayExpression array:
                    return new List<object>(array.Elements.Select(EvaluateExpression));
                case ArrayAccessExpression arrayAccess:
                    var arrayObj = EvaluateExpression(arrayAccess.Array);
                    var indexObj = EvaluateExpression(arrayAccess.Index);
                    if (arrayObj is List<object> list && indexObj != null)
                    {
                        int index = Convert.ToInt32(indexObj);
                        return index >= 0 && index < list.Count ? list[index] : null;
                    }
                    return null;
                default:
                    return null;
            }
        }

        private object EvaluateBinaryExpression(BinaryExpression binary)
        {
            var left = EvaluateExpression(binary.Left);
            var right = EvaluateExpression(binary.Right);

            switch (binary.Operator)
            {
                case "+":
                    return Convert.ToDouble(left) + Convert.ToDouble(right);
                case "-":
                    return Convert.ToDouble(left) - Convert.ToDouble(right);
                case "*":
                    return Convert.ToDouble(left) * Convert.ToDouble(right);
                case "/":
                    return Convert.ToDouble(left) / Convert.ToDouble(right);
                case "=":
                    return right;
                case "<":
                    return Convert.ToDouble(left) < Convert.ToDouble(right);
                case ">":
                    return Convert.ToDouble(left) > Convert.ToDouble(right);
                case "<=":
                    return Convert.ToDouble(left) <= Convert.ToDouble(right);
                case ">=":
                    return Convert.ToDouble(left) >= Convert.ToDouble(right);
                case "==":
                    return Convert.ToDouble(left) == Convert.ToDouble(right);
                case "!=":
                    return Convert.ToDouble(left) != Convert.ToDouble(right);
                case "&&":
                    return Convert.ToBoolean(left) && Convert.ToBoolean(right);
                case "||":
                    return Convert.ToBoolean(left) || Convert.ToBoolean(right);
                case "!":
                    return !Convert.ToBoolean(right);
                default:
                    return null;
            }
        }

        private object EvaluateFunctionCall(CallExpression call)
        {
            var args = call.Arguments.Select(EvaluateExpression).ToArray();
            return functionManager.CallFunction(call.FunctionName, args);
        }
    }
}
