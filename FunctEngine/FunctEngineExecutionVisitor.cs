

namespace FunctEngine;

public class FunctEngineExecutionVisitor : FunctEngineBaseVisitor<object>
{
    private RuntimeInterpreter interpreter;

        public FunctEngineExecutionVisitor(RuntimeInterpreter interpreter)
        {
            this.interpreter = interpreter;
        }

        public override object VisitVariableDeclaration(FunctEngineParser.VariableDeclarationContext context)
        {
            string varName = context.IDENTIFIER().GetText();
            object value = null;

            if (context.expression() != null)
            {
                value = Visit(context.expression());
            }

            interpreter.SetVariable(varName, value);
            return null;
        }

        public override object VisitAssignment(FunctEngineParser.AssignmentContext context)
        {
            string varName = context.IDENTIFIER().GetText();
            object value = Visit(context.expression());
            interpreter.SetVariable(varName, value);
            return value;
        }

        public override object VisitForStatement(FunctEngineParser.ForStatementContext context)
        {
            if (context.forInit() != null)
            {
                Visit(context.forInit());
            }

            // Bucle
            while (true)
            {
                // Verificar condición
                if (context.expression() != null)
                {
                    object condition = Visit(context.expression());
                    if (!IsTrue(condition)) break;
                }

                // Ejecutar cuerpo del bucle
                Visit(context.block());

                // Incremento/actualización
                if (context.forUpdate() != null)
                {
                    Visit(context.forUpdate());
                }
            }

            return null;
        }

        public override object VisitWhileStatement(FunctEngineParser.WhileStatementContext context)
        {
            while (true)
            {
                object condition = Visit(context.expression());
                if (!IsTrue(condition)) break;

                Visit(context.block());
            }

            return null;
        }
        public override object VisitForInit(FunctEngineParser.ForInitContext context)
        {
            if (context.variableDeclaration() != null)
            {
                return Visit(context.variableDeclaration());
            }
            else if (context.assignment() != null)
            {
                return Visit(context.assignment());
            }
            return null;
        }

        public override object VisitFunctionCall(FunctEngineParser.FunctionCallContext context)
        {
            string functionName = context.IDENTIFIER().GetText();
            var arguments = new List<object>();

            if (context.argumentList() != null)
            {
                foreach (var expr in context.argumentList().expression())
                {
                    arguments.Add(Visit(expr));
                }
            }

            return interpreter.CallExternalFunction(functionName, arguments.ToArray());
        }

        public override object VisitComparisonExpression(FunctEngineParser.ComparisonExpressionContext context)
        {
            if (context.ChildCount == 1)
                return Visit(context.GetChild(0));

            var left = Visit(context.addSubExpression(0));
            var right = Visit(context.addSubExpression(1));
            var op = context.GetChild(1).GetText();

            return op switch
            {
                "<" => Compare(left, right) < 0,
                ">" => Compare(left, right) > 0,
                "<=" => Compare(left, right) <= 0,
                ">=" => Compare(left, right) >= 0,
                "==" => Compare(left, right) == 0,
                "!=" => Compare(left, right) != 0,
                _ => throw new InvalidOperationException($"Operador no soportado: {op}")
            };
        }

        public override object VisitAddSubExpression(FunctEngineParser.AddSubExpressionContext context)
        {
            if (context.ChildCount == 1)
                return Visit(context.GetChild(0));

            var result = Visit(context.multDivExpression(0));
            
            for (int i = 1; i < context.ChildCount; i += 2)
            {
                var op = context.GetChild(i).GetText();
                var right = Visit(context.multDivExpression(i / 2 + 1));

                result = op switch
                {
                    "+" => Add(result, right),
                    "-" => Subtract(result, right),
                    _ => throw new InvalidOperationException($"Operador no soportado: {op}")
                };
            }

            return result;
        }

        public override object VisitMultDivExpression(FunctEngineParser.MultDivExpressionContext context)
        {
            if (context.ChildCount == 1)
                return Visit(context.GetChild(0));

            var result = Visit(context.atom(0));
            
            for (int i = 1; i < context.ChildCount; i += 2)
            {
                var op = context.GetChild(i).GetText();
                var right = Visit(context.atom(i / 2 + 1));

                result = op switch
                {
                    "*" => Multiply(result, right),
                    "/" => Divide(result, right),
                    _ => throw new InvalidOperationException($"Operador no soportado: {op}")
                };
            }

            return result;
        }

        public override object VisitAtom(FunctEngineParser.AtomContext context)
        {
            if (context.IDENTIFIER() != null)
            {
                return interpreter.GetVariable(context.IDENTIFIER().GetText());
            }
            else if (context.literal() != null)
            {
                return Visit(context.literal());
            }
            else if (context.functionCall() != null)
            {
                return Visit(context.functionCall());
            }
            else if (context.expression() != null)
            {
                return Visit(context.expression());
            }

            return null;
        }

        public override object VisitLiteral(FunctEngineParser.LiteralContext context)
        {
            if (context.NUMBER() != null)
            {
                string numberStr = context.NUMBER().GetText();
                return numberStr.Contains('.') ? double.Parse(numberStr) : int.Parse(numberStr);
            }
            else if (context.STRING() != null)
            {
                string str = context.STRING().GetText();
                
                // Determinar si usa comillas simples o dobles
                if (str.StartsWith("\"") && str.EndsWith("\""))
                {
                    // Comillas dobles: remover comillas y procesar escapes
                    str = str.Substring(1, str.Length - 2);
                    return ProcessStringEscapes(str);
                }
                else if (str.StartsWith("'") && str.EndsWith("'"))
                {
                    // Comillas simples: remover comillas y procesar escapes
                    str = str.Substring(1, str.Length - 2);
                    return ProcessStringEscapes(str);
                }
                
                return str; // Fallback
            }
            else if (context.BOOLEAN() != null)
            {
                return context.BOOLEAN().GetText() == "true";
            }
            else if (context.NULL() != null)
            {
                return null;
            }

            return null;
        }
        // Método auxiliar para procesar secuencias de escape en strings
        private string ProcessStringEscapes(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            return input
                .Replace("\\n", "\n")     // Nueva línea
                .Replace("\\t", "\t")     // Tabulación
                .Replace("\\r", "\r")     // Retorno de carro
                .Replace("\\\\", "\\")    // Barra invertida literal
                .Replace("\\\"", "\"")    // Comilla doble literal
                .Replace("\\'", "'");     // Comilla simple literal
        }
        // Métodos auxiliares para operaciones
        private bool IsTrue(object value)
        {
            if (value is bool b) return b;
            if (value is int i) return i != 0;
            if (value is double d) return d != 0.0;
            return value != null;
        }

        private int Compare(object left, object right)
        {
            if (left is IComparable leftComp && right is IComparable rightComp)
            {
                if (left.GetType() == right.GetType())
                    return leftComp.CompareTo(rightComp);
                
                // Convertir a double para comparación numérica
                if (IsNumeric(left) && IsNumeric(right))
                {
                    double leftNum = Convert.ToDouble(left);
                    double rightNum = Convert.ToDouble(right);
                    return leftNum.CompareTo(rightNum);
                }
            }
            
            return string.Compare(left?.ToString(), right?.ToString());
        }

        private object Add(object left, object right)
        {
            if (IsNumeric(left) && IsNumeric(right))
            {
                return Convert.ToDouble(left) + Convert.ToDouble(right);
            }
            return (left?.ToString() ?? "") + (right?.ToString() ?? "");
        }

        private object Subtract(object left, object right)
        {
            return Convert.ToDouble(left) - Convert.ToDouble(right);
        }

        private object Multiply(object left, object right)
        {
            return Convert.ToDouble(left) * Convert.ToDouble(right);
        }

        private object Divide(object left, object right)
        {
            double rightNum = Convert.ToDouble(right);
            if (rightNum == 0) throw new DivideByZeroException();
            return Convert.ToDouble(left) / rightNum;
        }

        private bool IsNumeric(object value)
        {
            return value is int || value is double || value is float || value is decimal;
        }
}