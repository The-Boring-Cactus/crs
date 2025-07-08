using Microsoft.CodeAnalysis.CSharp;
using FunctEngine.Ader.Text;
using System.Text;
using FunctEngine.Enums;
using FunctEngine.Exceptions;

namespace FunctEngine
{

    public class StatementEvaluator
    {
        Evaluator myEvaluator;
        Ader.Text.StringTokenizer tokenizer;

        public StatementEvaluator()
        {
            // Create the Evaluator
            myEvaluator = new Evaluator();

        }
        private string ConvertToLiteral(string input)
        {
            return SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(input)).ToFullString();
        }
        public bool EvaluateBooleanExpression(string scriptText, out bool resultingBool, VariableCollection varSpace)
        {
            string temp = scriptText;
            tokenizer = new Ader.Text.StringTokenizer(temp);
            tokenizer.IgnoreWhiteSpace = false;
            tokenizer.SymbolChars = new char[] { '+', '-', '/', '*', '>', '<', '(', ')', ';', '|', '=', '!', '&' };
            try
            {

                StringBuilder evalString = new StringBuilder();
                Token token;

                do
                {
                    token = tokenizer.Next();
                    if (token.Kind == TokenKind.WhiteSpace)
                    {
                        evalString.Append(token.Value);
                    }
                    else if (token.Kind == TokenKind.QuotedString)
                    {
                        evalString.Append(token.Value);
                    }
                    else if (token.Kind == TokenKind.Symbol)
                    {
                        evalString.Append(token.Value);
                    }
                    else if (token.Kind == TokenKind.Number)
                    {
                        evalString.Append(token.Value);
                    }
                    else if (token.Kind == TokenKind.Word)
                    {
                        evalString.Append(token.Value);
                    }
                    else if (token.Kind == TokenKind.VariableName)
                    {
                        FunctVariable myVar;
                        try
                        {
                            myVar = varSpace.getVariable(token.Value.Replace("$", ""));
                        }
                        catch (KeyNotFoundException)
                        {
                            GeneralParseException pge = new GeneralParseException("The Following Variable Was Not Found [" + token.Value + "] Trying to Evaluate Statement [" + scriptText + "]");
                            throw pge;
                        }

                        if (myVar.variableType == VariableType.String)
                        {

                            evalString.Append(ConvertToLiteral(myVar.Variable.ToString()));
                        }
                        else if (myVar.variableType == VariableType.Boolean)
                        {
                            evalString.Append(myVar.Variable.ToString().ToLower());
                        }
                        else
                        {
                            evalString.Append(myVar.Variable.ToString());
                        }

                    }
                    else
                    {
                        evalString.Append(token.Value);
                    }





                } while (token.Kind != TokenKind.EOF);


                object retVal = myEvaluator.EvalToObject(evalString.ToString());



                // Now, As our last step, we need to determine the type of variable
                // that we got back, and return it to the Funct executive...
                object retObj = new object();

                VariableType vt = DetermineVariableType(retVal.ToString(), ref retObj, evalString.ToString().Contains("\""));
                if (vt != VariableType.Boolean)
                {
                    resultingBool = false;
                    return false;
                }
                else
                {
                    resultingBool = (bool)retObj;
                    return true;
                }

            }

            catch (System.Exception ex)
            {
               Console.WriteLine(ex.Message);
                throw new GeneralParseException("Could Not Evaluate Boolean Expression [" + scriptText + "]");
            }
        }

        public bool Evaluate(string scriptText, VariableCollection varSpace)
        {
            string temp = scriptText;
            string returnVarName = StripReturnVariable(ref temp);

            // Create the Tokenizer
            tokenizer = new Ader.Text.StringTokenizer(temp);
            tokenizer.IgnoreWhiteSpace = false;
            tokenizer.SymbolChars = new char[] { '+', '-', '/', '*', '>', '<', '(', ')', ';', '|', '=', '!', '&' };




            try
            {

                StringBuilder evalString = new StringBuilder();
                Token token;
                // Now, Let's Loop Through Our 
                // 


                do
                {
                    token = tokenizer.Next();
                    if (token.Kind == TokenKind.WhiteSpace)
                    {
                        evalString.Append(token.Value);
                    }
                    else if (token.Kind == TokenKind.QuotedString)
                    {
                        evalString.Append(token.Value);
                    }
                    else if (token.Kind == TokenKind.Symbol)
                    {
                        evalString.Append(token.Value);
                    }
                    else if (token.Kind == TokenKind.Number)
                    {
                        evalString.Append(token.Value);
                    }
                    else if (token.Kind == TokenKind.Word)
                    {
                        evalString.Append(token.Value);
                    }
                    else if (token.Kind == TokenKind.VariableName)
                    {
                        FunctVariable myVar;
                        try
                        {
                            myVar = varSpace.getVariable(token.Value.Replace("$", ""));
                        }
                        catch (KeyNotFoundException)
                        {
                            GeneralParseException pge = new GeneralParseException("The Following Variable Was Not Found [" + token.Value + "] Trying to Evaluate Statement [" + scriptText + "]");
                            throw pge;
                        }

                        if (myVar.variableType == VariableType.String)
                        {
                            evalString.Append(ConvertToLiteral(myVar.Variable.ToString()));
                        }
                        else if (myVar.variableType == VariableType.Boolean)
                        {
                            evalString.Append(myVar.Variable.ToString().ToLower());
                        }
                        else
                        {
                            evalString.Append(myVar.Variable.ToString());
                        }

                    }
                    else
                    {
                        evalString.Append(token.Value);
                    }


                } while (token.Kind != TokenKind.EOF);



                object retVal = myEvaluator.EvalToObject(evalString.ToString());



                // Now, As our last step, we need to determine the type of variable
                // that we got back, and return it to the Funct executive...
                object retObj = new object();

                VariableType vt = DetermineVariableType(retVal.ToString(), ref retObj, evalString.ToString().Contains("\""));

                // Set the return variable
                FunctVariable sv = new FunctVariable(returnVarName.Replace("$", ""), vt, retObj);
                varSpace.setVariable(sv);





            }

            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw new GeneralParseException("Could Not Evaluate Statement [" + scriptText + "]");
            }


            return true;
        }

        private string StripReturnVariable(ref string scriptText)
        {
            string leftText = scriptText.Substring(0, scriptText.IndexOf("="));
            string rightText = scriptText.Substring(scriptText.IndexOf("=") + 1);
            scriptText = rightText.Trim();
            return leftText.Trim();
        }
        private VariableType DetermineVariableType(string variableContents, ref object ParsedObject, bool evalStringContainedQuote)
        {
            int parsedInt;
            double parsedFloat;
            bool parsedBool;

            // Try Bool First....
            bool worked = bool.TryParse(variableContents, out parsedBool);
            if (worked == true)
            {
                ParsedObject = parsedBool;
                return VariableType.Boolean;
            }

            // Try Int Next...
            worked = int.TryParse(variableContents, out parsedInt);
            if (worked == true)
            {
                if (evalStringContainedQuote == false)
                {
                    ParsedObject = parsedInt;
                    return VariableType.Integer;
                }
                else
                {
                    ParsedObject = variableContents;
                    return VariableType.String;
                }
            }

            // Try Float Next
            worked = double.TryParse(variableContents, out parsedFloat);
            if (worked == true)
            {
                if (evalStringContainedQuote == false)
                {
                    ParsedObject = parsedFloat;
                    return VariableType.Float;
                }
                else
                {
                    ParsedObject = variableContents;
                    return VariableType.String;
                }
            }
            else
            {
                ParsedObject = variableContents;
                return VariableType.String;
            }


        }
    }
}
