using System.Collections;
using System.Text;
using FunctEngine.Enums;
using FunctEngine.Exceptions;

namespace FunctEngine
{
    public class FunctScriptInterpreter
    {
        private ScriptLine currentScriptLine;
        public ScriptLine CurrentScriptLine
        {
            get { return currentScriptLine; }
        }
        private string scriptContent;
        private VariableCollection variableCollection;
        private int curScriptLine = -1;
        private List<ScriptLine> scriptArray;
        private List<FunctionInstance> functionList;
        private List<ScriptLine> labelLocations;
        private List<CompilerError> compilerErrors;
        StatementEvaluator statementEvaluator;
        private Stack<WhileLoopInstance> whileLoopStack;
        private List<WhileLoopInstance> whileLoopList;
        private FunctList mapList;
        private ScriptStatus status;
        private string FunctName;
        private Stack scriptStack;
        public FunctScriptInterpreter(string scriptContentin, VariableCollection varSpace,FunctList mappingList, StatementEvaluator statementEvaluator, string FunctName)
        {
            variableCollection = varSpace;
            status = ScriptStatus.Ready;
            mapList = mappingList;
            scriptArray = new List<ScriptLine>();
            scriptStack = new Stack();
            this.statementEvaluator = statementEvaluator;
            this.whileLoopStack = new Stack<WhileLoopInstance>();
            this.whileLoopList = new List<WhileLoopInstance>();
            scriptContent = scriptContentin;
            this.FunctName= FunctName;
            compilerErrors = new List<CompilerError>();
        }
        /// <summary>
		/// Sets the script mapping agent.
		/// </summary>
		public FunctList MappingList
        {
            set
            {
                this.mapList = value;
            }

        }
        /// <summary>
		/// Gets/Sets the Funct script file
		/// </summary>
		public string ScriptFile
        {
            get
            {
                return scriptContent;
            }
            set
            {
                scriptContent = value;
            }
        }
        /// <summary>
		/// The status of the script.
		/// </summary>
		public ScriptStatus Status
        {
            get
            {
                return this.status;
            }
            set
            {
                this.status = value;
            }

        }

        public bool InitializeScript(ref List<CompilerError> compilerErrors)
        {
            scriptArray.Clear();
            scriptStack.Clear();
            whileLoopStack.Clear();
            whileLoopList.Clear();
            scriptArray = ReadScriptContent(this.scriptContent);
                

            
            ScriptLine sl = new ScriptLine();
            sl.ScriptLineType = ScriptLineType.Comment;
            sl.Text = "//START";
            scriptArray.Insert(0, sl);
            currentScriptLine = scriptArray[0];
            return true;
        }

        /// <summary>
        /// This function loops through the current function list
        /// and sets all the closing block locations...
        /// </summary>
        private void SetClosingBlockOfFunctions()
        {
            foreach (FunctionInstance fi in functionList)
            {
                fi.EndLocation = FindCodeBlockEndLocation(fi.FunctionScriptLine);
            }


        }


        /// <summary>
        /// Opens the Funct script
        /// </summary>
        public List<ScriptLine> ReadScriptContent(string contScriptText)
        {
            functionList = new List<FunctionInstance>();
            labelLocations = new List<ScriptLine>();


            List<ScriptLine> retList = new List<ScriptLine>();

            string[] scriptContent = contScriptText.Split(new char[] {'\n'}, StringSplitOptions.RemoveEmptyEntries);
           
            
            
            for (int lineNumber = 0;  lineNumber< scriptContent.Length; lineNumber++)
            {
                string curScriptText = scriptContent[lineNumber];
                
                curScriptText = curScriptText.Trim();
                // If We Have a comment line, let's just not include it in the script 
                ScriptLine sl = new ScriptLine();
                sl.LineNumber = lineNumber + 1;
                sl.OriginalText = curScriptText;
                sl.Text = FunctScriptInterpreter.RemoveCommentFromLine(curScriptText.Trim());
                sl.FunctName = FunctName;
                retList.Add(sl);
            }
            
            this.curScriptLine = 0;


            // OK, Here we Create a List of Lists, and go through each file using
            // tail recursion to grab all the include files....
            List<List<ScriptLine>> includeList = new List<List<ScriptLine>>();
    
            foreach (List<ScriptLine> lineList in includeList)
            {
                retList.AddRange(lineList);
            }



            int count = 0;
            foreach (ScriptLine sl in retList)
            {

                count++;
                sl.LineNumber = count;
                ScriptLineType lt = FunctScriptInterpreter.AnalyzeScriptLine(sl, mapList);
                sl.ScriptLineType = lt;
                if (lt == ScriptLineType.Function)
                {
                    FunctionInstance fi = new FunctionInstance();
                    fi.FunctionScriptLine = sl;
                    fi.FunctionName = GetScriptFunctionNameFromScriptLine(sl);
                    fi.StartLocation = sl.LineNumber;
                    this.functionList.Add(fi);
                }
                if (lt == ScriptLineType.Label)
                {
                    this.labelLocations.Add(sl);
                }
            }



            



            return retList;
        }


        /// <summary>
        /// This function extracts the name of a Script Function (not the Funct function)
        /// from the current script line
        /// </summary>
        /// <param name="sl"></param>
        private string GetScriptFunctionNameFromScriptLine(ScriptLine sl)
        {
            try
            {
                string text = sl.Text;
                text = text.Substring(8).Trim();
                text = text.Replace("()", "");
                text = text.Trim();
                return text;
            }
            catch (System.Exception)
            {
                throw new GeneralParseException("Could Not Extract the Script Function Name From [" + sl.OriginalText + "]");
            }
        }


        /// <summary>
        /// Gets the next script line in the Funct script.
        /// </summary>
        /// <returns>Returns a LineType indicating the type of the script line</returns>
        private ScriptLine GetNextScriptLine()
        {
            try
            {
                return scriptArray[currentScriptLine.LineNumber + 1];
            }
            catch (System.ArgumentOutOfRangeException)
            {
                ScriptLine newLine = new ScriptLine();
                newLine.OriginalText = "End;";
                newLine.Text = "End;";
                newLine.ScriptLineType = ScriptLineType.End;
                newLine.LineNumber = curScriptLine;
                return newLine;
            }
        }

        /// <summary>
        /// Gets the next Funct to execute.
        /// </summary>
        public void GetNextFunct()
        {


            int exit = 0;
            while (exit == 0)
            {
                currentScriptLine = this.GetNextScriptLine();


                if (currentScriptLine.ScriptLineType == ScriptLineType.Blank)
                {
                    continue;
                }
                if (currentScriptLine.ScriptLineType == ScriptLineType.End)
                {
                    break;
                }
                if (currentScriptLine.ScriptLineType == ScriptLineType.LeftBracket)
                {
                    continue;
                }
                if (currentScriptLine.ScriptLineType == ScriptLineType.RightBracket)
                {
                    DetermineWhileLoopState();
                    continue;
                }
                if (currentScriptLine.ScriptLineType == ScriptLineType.Label)
                {
                    continue;
                }
                if (currentScriptLine.ScriptLineType == ScriptLineType.Comment)
                {
                    continue;
                }
                if (currentScriptLine.ScriptLineType == ScriptLineType.Break)
                {
                    ExecuteBreakStatement();
                    continue;
                }
                if (currentScriptLine.ScriptLineType == ScriptLineType.Continue)
                {
                    ExecuteContinueStatement();
                    continue;
                }

                if (currentScriptLine.ScriptLineType == ScriptLineType.Unknown)
                {
                    ScriptLine errorLine = currentScriptLine;
                    throw (new GeneralParseException("Error Occured Parsing Script [" + errorLine.FunctName + "]  Statement =   :['" + errorLine.OriginalText + "]'"));
                }
                if (currentScriptLine.ScriptLineType == ScriptLineType.Goto)
                {
                    this.JumpToGoto();
                    continue;
                }
                if (currentScriptLine.ScriptLineType == ScriptLineType.Call)
                {
                    this.JumpToCall();
                    continue;
                }
                if (currentScriptLine.ScriptLineType == ScriptLineType.ScriptIf)
                {
                    this.ExecuteIfStatement();
                    continue;
                }
                if (currentScriptLine.ScriptLineType == ScriptLineType.If)
                {
                    this.ExecuteTraditionalIfStatement();
                    continue;
                }
                if (currentScriptLine.ScriptLineType == ScriptLineType.WhileLoop)
                {
                    this.ExecuteWhileLoopStatement();
                    continue;
                }
                if (currentScriptLine.ScriptLineType == ScriptLineType.Return)
                {
                    int temp;
                    try
                    {
                        temp = (int)scriptStack.Pop();
                    }
                    catch (System.InvalidOperationException)
                    {
                        break;
                    }

                    // Also, if we've hit a return statement, and we are inside of a while loop (or while loops)
                    // We need to actually clear the entire while loop stack for all the given
                    // while loops that actually exist inside of the function that we are currently in before we 
                    // change the Funct script execution location...
                    ClearWhileLoopStackForCurrentFunction();

                    this.currentScriptLine = scriptArray[temp];


                    continue;

                }
                if (currentScriptLine.ScriptLineType == ScriptLineType.EvaluaFunctatement)
                {
                    statementEvaluator.Evaluate(currentScriptLine.Text, variableCollection);
                    continue;
                }

                if (currentScriptLine.ScriptLineType == ScriptLineType.Funct)
                {
                   // FunctResult FunctResult = this.ExecuteCurrentFunct();
                    
                    return;
                }


                continue;
            }
            this.status = ScriptStatus.Finished;

        }

        // This function is called when we hit a right bracket statement
        // We basically need to see if we are in a while loop,
        // and if we are, we need to go back to the beginning of the while loop...
        // if we aren
        private void DetermineWhileLoopState()
        {
            foreach (WhileLoopInstance wli in whileLoopList)
            {
                if (wli.EndLocation == currentScriptLine.LineNumber)
                {
                    // OK, we actually are in a while loop instance, so let's go back to the beginning of it.
                    // We subtract 1 from the start location to make sure the while loop
                    // hits the while($something) line that we care about....
                    currentScriptLine = scriptArray[wli.StartLocation - 1];
                    return;
                }
            }
        }

        /// <summary>
        /// This function is called when we hit a return statement...
        /// if we are inside a function, we need to verify that we
        /// close out all the while loops from the stack for the current
        /// function
        /// </summary>
        private void ClearWhileLoopStackForCurrentFunction()
        {
            // OK, Need to do this here...
            ScriptLine sl = currentScriptLine;
            foreach (FunctionInstance fi in functionList)
            {
                if (sl.LineNumber >= fi.StartLocation && sl.LineNumber <= fi.EndLocation)
                {
                    // OK, We are inside this function....
                    //
                    // we need to go through the while loop stack and remove any of them where they start of the while loop
                    // is inside of this current function...
                    bool inCurrentFunction = true;
                    while (inCurrentFunction == true)
                    {
                        if (whileLoopStack.Count == 0) return;
                        WhileLoopInstance wli = whileLoopStack.Peek();
                        if (wli.StartLocation >= sl.LineNumber && wli.EndLocation <= sl.LineNumber)
                        {
                            // OK, We Need to Pop This Item off the stack
                            whileLoopStack.Pop();
                        }
                        else
                        {
                            // OK, The Next while loop down is outside the function, so we can just return from here
                            return;
                        }
                    }
                }

            }

        }

        /// <summary>
        /// Analyzes the script line and returns the line type.
        /// </summary>
        /// <returns>Returns the LineType</returns>
        public static ScriptLineType AnalyzeScriptLine(ScriptLine sl, FunctList mappingList)
        {
            string scriptText = sl.Text;


            if (FunctScriptInterpreter.LineIsBlank(sl) == true)
            {
                return ScriptLineType.Blank;
            }
            if (FunctScriptInterpreter.LineIsComment(sl) == true)
            {
                return ScriptLineType.Comment;
            }
            if (FunctScriptInterpreter.LineIsEnd(sl) == true)
            {
                return ScriptLineType.End;
            }
            if (FunctScriptInterpreter.LineIsReturn(sl) == true)
            {
                return ScriptLineType.Return;
            }
            if (FunctScriptInterpreter.LineIsBreak(sl) == true)
            {
                return ScriptLineType.Break;
            }
            if (FunctScriptInterpreter.LineIsContinue(sl) == true)
            {
                return ScriptLineType.Continue;
            }
            if (FunctScriptInterpreter.LineIsFunct(sl, mappingList) == true)
            {
                return ScriptLineType.Funct;
            }
            if (FunctScriptInterpreter.LineIsFunctionDefinition(sl) == true)
            {
                return ScriptLineType.Function;
            }
            if (FunctScriptInterpreter.LineIsLabel(sl) == true)
            {
                return ScriptLineType.Label;
            }
            if (FunctScriptInterpreter.LineIsGoto(sl) == true)
            {
                return ScriptLineType.Goto;
            }
            if (FunctScriptInterpreter.LineIsCall(sl) == true)
            {
                return ScriptLineType.Call;
            }
            if (FunctScriptInterpreter.LineIsRightBracket(sl) == true)
            {
                return ScriptLineType.RightBracket;
            }
            if (FunctScriptInterpreter.LineIsLeftBracket(sl) == true)
            {
                return ScriptLineType.LeftBracket;
            }
            if (FunctScriptInterpreter.LineIsScriptIf(sl) == true)
            {
                return ScriptLineType.ScriptIf;
            }
            if (FunctScriptInterpreter.LineIsIfStatement(sl) == true)
            {
                return ScriptLineType.If;
            }
            if (FunctScriptInterpreter.LineIsWhileStatement(sl) == true)
            {
                return ScriptLineType.WhileLoop;
            }
            if (FunctScriptInterpreter.LineIsEvaluaFunctatement(sl) == true)
            {
                return ScriptLineType.EvaluaFunctatement;
            }

            return ScriptLineType.Unknown;
        }

        private static bool LineIsComment(ScriptLine sl)
        {

            if (sl.Text.IndexOf("//") == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private static bool LineIsBlank(ScriptLine sl)
        {
            if (sl.Text.Length == 0) return true;
            else return false;
        }


        /// <summary>
        /// This is an internal function used to determine if a script line is a termination command.
        /// </summary>
        /// <returns>true or false</returns>
        private static bool LineIsEnd(ScriptLine sl)
        {

            if (sl.Text.Length < 3) return false;
            if (sl.Text.Substring(0, 3) == "End") return true;
            else return false;
        }

        private static bool LineIsContinue(ScriptLine sl)
        {

            if (sl.Text.Length < 8) return false;
            if (sl.Text.Substring(0, 8) == "continue") return true;
            else return false;
        }

        private static bool LineIsBreak(ScriptLine sl)
        {
            if (sl.Text.Length < 5) return false;
            if (sl.Text.Substring(0, 5) == "break") return true;
            else return false;
        }

      



        /// <summary>
        /// This is an internal function used to determine if a script line is a return command.
        /// </summary>
        /// <returns>true or false</returns>
        private static bool LineIsReturn(ScriptLine sl)
        {

            if (sl.Text.Length < 6) return false;
            if (sl.Text.Substring(0, 6) == "return") return true;
            else return false;
        }



        /// <summary>
        /// This determines if the current script line is a label command.
        /// </summary>
        /// <returns></returns>
        private static bool LineIsLabel(ScriptLine sl)
        {

            if (sl.Text.Length < 6) return false;

            if ((sl.Text.Substring(0, 6) == "Label ") && (sl.Text.IndexOf(":") != -1))
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        /// <summary>
        /// This determines if the current script line is a function definition
        /// </summary>
        /// <returns></returns>
        private static bool LineIsFunctionDefinition(ScriptLine sl)
        {

            if (sl.Text.Length < 9) return false;

            if ((sl.Text.Substring(0, 9) == "function ") && (sl.Text.IndexOf('(') != -1) && (sl.Text.IndexOf(')') != -1))
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        /// <summary>
        /// This determines if a script line is a ScriptIf statement.
        /// </summary>
        /// <returns></returns>
        private static bool LineIsScriptIf(ScriptLine sl)
        {

            if (sl.Text.Length < 9) return false;
            if (sl.Text.Substring(0, 9) == "ScriptIf(")
            {
                return true;
            }
            else
            {
                return false;
            }
        }



        /// <summary>
        /// This determines if the script statement is a call command.
        /// </summary>
        /// <returns></returns>
        private static bool LineIsCall(ScriptLine sl)
        {

            if (sl.Text.Length < 5) return false;
            if (sl.Text.Substring(0, 5) == "call ")
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// This determines if the script statement is a goto command.
        /// </summary>
        /// <returns></returns>
        private static bool LineIsGoto(ScriptLine sl)
        {

            if (sl.Text.Trim().Length < 5) return false;
            if (sl.Text.Trim().Substring(0, 5) == "goto ")
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// This determines if the line is a left bracket.
        /// </summary>
        /// <returns></returns>
        private static bool LineIsLeftBracket(ScriptLine sl)
        {
            if ((sl.Text.Length == 1) && (sl.Text[0] == '{'))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// This determines if the line is a right bracket.
        /// </summary>
        /// <returns></returns>
        private static bool LineIsRightBracket(ScriptLine sl)
        {
            if ((sl.Text.Length == 1) && (sl.Text[0] == '}'))
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        /// <summary>
        /// This determines if the line is a actual Funct command.
        /// </summary>
        /// <returns></returns>
        private static bool LineIsFunct(ScriptLine sl, FunctList mapList)
        {
            string temp = sl.Text;

            temp = temp.Trim();
            // Ok, For A Line To Be A Funct, it must have the following
            // characteristics...
            // 1. It must have at least one set of parantheses.
            // 2. It must match at least one name in the MappingList.

            if (temp.IndexOf("(") == -1)
            {
                return false;
            }
            if (temp.IndexOf(")") == -1)
            {
                return false;
            }

            // OK, Here We Need To Remove All Characters From Inside Quote Marks...
            temp = FunctScriptInterpreter.RemoveQuotedCharacters(temp);

            // We'll If We Made it here, it's starting to look like this line is a Funct.

            // If it has an equals, lets chop it off it and everything before it.
            if (temp.IndexOf("=") != -1)
            {
                int equalloc = temp.IndexOf("=");
                temp = temp.Substring(equalloc + 1);
            }

            // 
            if (temp.IndexOf("(") != -1)
            {
                temp = temp.Substring(0, temp.IndexOf("("));
            }



            // Ok, Finally Let's see if this is in the FunctList.
            temp = temp.Trim();
            if (mapList.FunctExists(temp))
            {
                //Assign The name of the current Funct to the variable.
                sl.FunctFunctionName = temp;
                return true;

            }
            else
            {
                return false;
            }

        }

        private static bool LineIsEvaluaFunctatement(ScriptLine sl)
        {

            // Ok, For A Line To Be An Evaluate Statement, it must have the following
            // characteristics...
            // 1. It must contain a single Variable on the Left of the Equals Sign
            // 2. It must contain an equals sign...
            // 3. The text on the right can be anything to be evaulated...
            // 4. It must contain a semicolon at the end...


            // If we don't have an Equals we bail
            if (sl.Text.Trim().IndexOf("=") == -1)
            {
                return false;
            }

            if (sl.Text.Trim().IndexOf("$") != 0)
            {
                return false;
            }

            if (sl.Text.Trim().IndexOf(";") == -1)
            {
                return false;
            }



            // OK, Here We Need To Remove All Characters From Inside Quote Marks...
            string temp = RemoveQuotedCharacters(sl.Text);

            // We'll If We Made it here, it's starting to look like this line is a Funct.

            // lets chop it off the equals it and everything before it.
            int equalloc = temp.IndexOf("=");
            temp = temp.Substring(equalloc + 1);

            // Now, if what's left has either a + - / * symbol, it must be a 
            // statement for evaluation

            if (temp.IndexOf(";") > -1)
            {
                return true;
            }

            if (temp.IndexOf("+") > -1)
            {
                return true;
            }
            if (temp.IndexOf("-") > -1)
            {
                return true;
            }
            if (temp.IndexOf("/") > -1)
            {
                return true;
            }
            if (temp.IndexOf("*") > -1)
            {
                return true;
            }

            if (temp.IndexOf(">") > -1)
            {
                return true;
            }

            if (temp.IndexOf("<") > -1)
            {
                return true;
            }

            // If we don't have an operator, it must not be a statement...
            return false;

        }

        private static bool LineIsIfStatement(ScriptLine sl)
        {
            // Ok, For A Line To Be An If Statement, it must have the following
            // characteristics...
            // 1. It must start with if
            // 2. It must contain and opening and closing paranethes
            // 3. It should not contain a semicolon at the end
            // 4. The last character should be a ')'

            if (sl.Text.Trim().IndexOf("if") != 0)
            {
                return false;
            }

            string temp = RemoveQuotedCharacters(sl.Text);
            if (temp.Trim()[temp.Trim().Length - 1] != ')')
            {
                return false;
            }

            if (temp.IndexOf(';') != -1)
            {
                return false;
            }

            if (temp.IndexOf('(') == -1)
            {
                return false;
            }

            if (temp.IndexOf(')') == -1)
            {
                return false;
            }

            // If it passed all this, it's probably an "If" statement:)
            return true;
        }

        private static bool LineIsWhileStatement(ScriptLine sl)
        {
            // Ok, For A Line To Be An While Statement, it must have the following
            // characteristics...
            // 1. It must start with While
            // 2. It must contain and opening and closing paranethes
            // 3. It should not contain a semicolon at the end
            // 4. The last character should be a ')'

            if (sl.Text.Trim().IndexOf("while") != 0)
            {
                return false;
            }

            string temp = RemoveQuotedCharacters(sl.Text);
            if (temp.Trim()[temp.Trim().Length - 1] != ')')
            {
                return false;
            }

            if (temp.IndexOf(';') != -1)
            {
                return false;
            }

            if (temp.IndexOf('(') == -1)
            {
                return false;
            }

            if (temp.IndexOf(')') == -1)
            {
                return false;
            }

            // If it passed all this, it's probably an "While" statement:)
            return true;
        }



        /// <summary>
        /// This function handles jumping to a goto location.
        /// </summary>
        public void JumpToGoto()
        {
            string temp;
            temp = currentScriptLine.Text;
            // Now Let's Find Out Which Function line we need to go to...

            if (temp.Substring(0, 5) != "goto ")
            {
                throw (new GeneralParseException("An Error Occured Processing Goto: " + temp));
            }

            temp = temp.Remove(0, 5);
            temp = temp.Trim();
            try
            {
                temp = temp.Substring(0, temp.IndexOf(';'));
            }
            catch (System.ArgumentOutOfRangeException)
            {
                throw new GeneralParseException("Invalid Goto Syntax (Script Line Missing Semicolon) [" + temp + "]");
            }
            this.curScriptLine = 0;
            int found = 0;
            foreach (ScriptLine sl in labelLocations)
            {
                string tempString = sl.Text;
                tempString = tempString.Trim();
                if ((tempString.IndexOf("Label " + temp + ":") != -1))
                {
                    currentScriptLine = sl;
                    found = 1;
                    break;
                }
            }
            if (found == 0)
            {
                throw new GeneralParseException("Could Not Locate Function [" + temp + "]");
            }
        }

        /// <summary>
        /// This function handles jumping to a call statement.
        /// </summary>
        public void JumpToCall()
        {
            string temp;
            temp = currentScriptLine.Text;
            // Now Let's Find Out Which Function line we need to go to...

            if (temp.Substring(0, 5) != "call ")
            {
                throw (new GeneralParseException("An Error Occured Processing Call: " + temp));
            }
            temp = temp.Remove(0, 5);
            temp = temp.Trim();
            try
            {
                temp = temp.Substring(0, temp.IndexOf('('));
            }
            catch (System.ArgumentOutOfRangeException)
            {
                throw new GeneralParseException("Invalid Call Syntax (Function Missing Left Parenthesis) [" + temp + "]");
            }
            int hold;
            hold = this.currentScriptLine.LineNumber;
            scriptStack.Push(hold);
            int found = 0;
            foreach (FunctionInstance fi in functionList)
            {
                if ((fi.FunctionName == temp))
                {
                    currentScriptLine = fi.FunctionScriptLine;
                    found = 1;
                    break;
                }
            }
            if (found == 0)
            {
                throw new GeneralParseException("Could Not Located Function [" + temp + "]");
            }
        }

        /// <summary>
        /// This function handles jumping to a debug function for 
        /// the debug mode of the Funct executive.
        /// </summary>
        public void JumpToDebugFunction()
        {
            string temp;
            temp = currentScriptLine.Text;
            // Now Let's Find Out Which Function line we need to go to...

            if (temp.Substring(0, 5) != "call ")
            {
                throw (new GeneralParseException("An Error Occured Processing Call: " + temp));
            }
            temp = temp.Remove(0, 5);
            temp = temp.Replace(";", ""); // Remove the trailing semicolon


            this.curScriptLine = 0;
            currentScriptLine = this.FindCallLocation(temp);
            if (currentScriptLine == null)
            {
                throw new GeneralParseException("Could Not Located Function [" + temp + "]");
            }
        }


        /// <summary>
        /// This function executes a script if command
        /// </summary>
        public void ExecuteIfStatement()
        {
            string temp;
            temp = currentScriptLine.Text;
            // Now Let's Find Out Which Function line we need to go to...
            try
            {
                bool worked = LoadParameters();
            }
            catch (ArgumentParseException)
            {
                throw new GeneralParseException("Failed To Load Function Argument Parameters For Script Line: [" + currentScriptLine.Text + "]");
            }

            // Now Let's Access All The Variables that should be queued up.
            // Argument0 =  Boolean True or False
            // Argument1 =  CALL or GOTO

            FunctVariable arg0 = new FunctVariable();
            try
            {
                arg0 = this.variableCollection.getVariable("Argument0");
            }
            catch (System.ArgumentOutOfRangeException)
            {
                throw new GeneralParseException("Could Not Obtain First Argument to ScriptIf Function On Line [" + currentScriptLine.Text + "]");
            }
            catch (ArgumentParseException)
            {
                throw new GeneralParseException("Could Not Obtain First Argument to ScriptIf Function On Line [" + currentScriptLine.Text + "]");
            }
            string arg0String = "";
            arg0String = arg0.Variable.ToString();
            if (arg0String.ToUpper() != "TRUE" && arg0String.ToUpper() != "FALSE")
            {
                throw new GeneralParseException("Could Not Evaluate Boolean from [" + arg0String + "] On Line [" + curScriptLine.ToString() + "]");
            }
            FunctVariable arg1 = new FunctVariable();
            try
            {
                arg1 = this.variableCollection.getVariable("Argument1");
            }
            catch (System.ArgumentOutOfRangeException)
            {
                throw new GeneralParseException("Could Not Obtain Second Argument (CALL,GOTO) To ScriptIf Function On Line [" + curScriptLine.ToString() + "]");
            }
            catch (ArgumentParseException)
            {
                throw new GeneralParseException("Could Not Obtain Second Argument (CALL,GOTO) To ScriptIf Function On Line [" + curScriptLine.ToString() + "]");
            }

            string arg1String = "";
            arg1String = arg1.Variable.ToString();
            if (arg1String.ToUpper() != "CALL" && arg1String.ToUpper() != "GOTO")
            {
                throw new GeneralParseException("Second Argument to ScriptIf Invalid On Line [" + curScriptLine.ToString() + "] Must Be Either CALL or GOTO");
            }

            // True Location
            FunctVariable arg2 = new FunctVariable();
            string arg2String = "";
            try
            {
                arg2 = this.variableCollection.getVariable("Argument2");
            }
            catch (System.ArgumentOutOfRangeException)
            {
                throw new GeneralParseException("Could Not Obtain Second Argument (TRUE LOCATION) To ScriptIf Function On Line [" + curScriptLine.ToString() + "]");
            }
            catch (ArgumentParseException)
            {
                throw new GeneralParseException("Could Not Obtain Second Argument (TRUE LOCATION) To ScriptIf Function On Line [" + curScriptLine.ToString() + "]");
            }
            arg2String = arg2.Variable.ToString();

            // False Location
            FunctVariable arg3 = new FunctVariable();
            string arg3String = "";
            try
            {
                arg3 = this.variableCollection.getVariable("Argument3");
            }
            catch (System.ArgumentOutOfRangeException)
            {
                throw new GeneralParseException("Could Not Obtain Third Argument (FALSE LOCATION) To ScriptIf Function On Line [" + curScriptLine.ToString() + "]");
            }
            catch (ArgumentParseException)
            {
                throw new GeneralParseException("Could Not Obtain Third Argument (FALSE LOCATION) To ScriptIf Function On Line [" + curScriptLine.ToString() + "]");
            }
            arg3String = arg3.Variable.ToString();

            // OK, We've got all the variables, now we need to decide what to do...

            if (arg0String.ToUpper() == "TRUE")
            {
                if (arg2String == "") return; // If it's blank, lets just bail.
                if (arg1String == "CALL")
                {
                    int hold = this.currentScriptLine.LineNumber;
                    scriptStack.Push(hold);
                    currentScriptLine = FindCallLocation(arg2String.Trim());
                }
                else
                {
                    currentScriptLine = FindGotoLocation(arg2String.Trim());
                }
            }
            if (arg0String.ToUpper() == "FALSE")
            {
                if (arg3String == "") return; // If it's blank, lets just bail.
                if (arg1String == "CALL")
                {
                    int hold = this.currentScriptLine.LineNumber;
                    scriptStack.Push(hold);
                    currentScriptLine = FindCallLocation(arg3String.Trim());
                }
                else
                {
                    currentScriptLine = FindGotoLocation(arg3String.Trim());
                }
            }

        }


        public void ExecuteTraditionalIfStatement()
        {
            string temp;
            string originalStatement = currentScriptLine.Text;
            temp = currentScriptLine.Text;

            // We Already Know beyond a resonable doubt that we have an
            // if statement, so let's start tearing it apart.

            int startLoc = temp.IndexOf('(');
            int endLoc = temp.LastIndexOf(')');
            if (startLoc == -1 || endLoc == -1 || endLoc <= startLoc)
            {
                throw new GeneralParseException("The Following If Statement Could Not Be Evaluated : " + temp);
            }

            temp = temp.Substring(startLoc + 1, (endLoc - startLoc) - 1); // Temp Should Now Contain the contents of the If Statement to be evaluated;

            // OK, Now We Just need to evaluate what's in there and see if it's a valid true-false statement

            bool resultingBool = false;
            bool worked = statementEvaluator.EvaluateBooleanExpression(temp, out resultingBool, variableCollection);

            if (worked)
            {
                if (resultingBool == true)
                {
                    return; // We just need to keep on going to the next line of the script
                }
                else
                {
                    // If the Resulting Bool was false, we need to skip the {} code block and 
                    // start on the next line

                    // Actually, this is a bit more complicated:(  We need to supported
                    // nested IF statements which means that we need to increment
                    // for each if statement contained in the block...


                    int loc = currentScriptLine.LineNumber;
                    int levelCount = 0; // This is the current cyclometric depth

                    try
                    {
                        while ((scriptArray[loc].ScriptLineType != ScriptLineType.RightBracket) || (levelCount != 1))
                        {
                            if (scriptArray[loc].ScriptLineType == ScriptLineType.LeftBracket)
                            {
                                levelCount++;
                            }
                            if (scriptArray[loc].ScriptLineType == ScriptLineType.RightBracket)
                            {
                                levelCount--;
                            }
                            loc++;
                        }
                        currentScriptLine = scriptArray[loc];
                        return;
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        throw new GeneralParseException("The Closing Block of the If Statement [" + originalStatement + "] Could Not Be Found");
                    }
                }
            }
            else
            {
                throw new GeneralParseException("The Following If Statement Could Not Be Evaluated : " + temp);
            }

        }

        public void ExecuteWhileLoopStatement()
        {
            string temp;
            string originalStatement = currentScriptLine.Text;
            temp = currentScriptLine.Text;

            // We Already Know beyond a resonable doubt that we have an
            // if statement, so let's start tearing it apart.

            int startLoc = temp.IndexOf('(');
            int endLoc = temp.LastIndexOf(')');
            if (startLoc == -1 || endLoc == -1 || endLoc <= startLoc)
            {
                throw new GeneralParseException("The Following While Statement Expression Could Not Be Evaluated : " + temp);
            }

            temp = temp.Substring(startLoc + 1, (endLoc - startLoc) - 1); // Temp Should Now Contain the contents of the While Loop Statement to be evaluated;

            // OK, Now We Just need to evaluate what's in there and see if it's a valid true-false statement

            bool resultingBool = false;
            bool worked = statementEvaluator.EvaluateBooleanExpression(temp, out resultingBool, variableCollection);

            if (worked)
            {
                if (resultingBool == true)
                {
                    WhileLoopInstance instance = SearchWhileLoopListForMatchingStartLocation(currentScriptLine);
                    // OK, since we are now in a while loop, let's push this on to the while loop stack
                    // if we aren't already in this while loop from a previous execution
                    if (whileLoopStack.Count > 0)
                    {
                        WhileLoopInstance wli = whileLoopStack.Peek();
                        if (wli.StartLocation != currentScriptLine.LineNumber)
                        {
                            // OK, we are entering a new while loop, so let's push it on the stack
                            whileLoopStack.Push(instance);
                        }
                    }
                    else
                    {
                        whileLoopStack.Push(instance);
                    }
                    return; // We just need to keep on going to the next line of the script
                }
                else
                {
                    // If the Resulting Bool was false, we need to skip the {} code block and 
                    // start on the next line

                    // ALSO, if we were executing the while loop, and we had the current while 
                    // loop on the stack, we need to pop it off...

                    // Actually, this is a bit more complicated:(  We need to supported
                    // nested While and If statements which means that we need to increment
                    // for each opening { contained in the block...

                    // OK, Let's pop the stack if we need to do it.
                    if (whileLoopStack.Count > 0)
                    {
                        WhileLoopInstance wli = whileLoopStack.Peek();
                        if (wli.StartLocation == currentScriptLine.LineNumber)
                        {
                            whileLoopStack.Pop(); // OK, Popping the stack
                            // since we are now out of the while loop;
                        }

                    }


                    // This Gets us to where we need to be

                    int loc = currentScriptLine.LineNumber;
                    int levelCount = 0; // This is the current cyclometric depth

                    try
                    {
                        while ((scriptArray[loc].ScriptLineType != ScriptLineType.RightBracket) || (levelCount != 1))
                        {
                            if (scriptArray[loc].ScriptLineType == ScriptLineType.LeftBracket)
                            {
                                levelCount++;
                            }
                            if (scriptArray[loc].ScriptLineType == ScriptLineType.RightBracket)
                            {
                                levelCount--;
                            }
                            loc++;
                        }
                        currentScriptLine = scriptArray[loc];
                        return;
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        throw new GeneralParseException("The Closing Block of the While Loop Statement [" + originalStatement + "] Could Not Be Found");
                    }
                }
            }
            else
            {
                throw new GeneralParseException("The Following While Statement Could Not Be Evaluated : " + temp);
            }

        }

        /// <summary>
        /// This function handles executing the 'break' keyword in the Funct
        /// script.
        /// </summary>
        public void ExecuteBreakStatement()
        {

            if (whileLoopStack.Count == 0) return;
            int currentLocation = currentScriptLine.LineNumber;
            WhileLoopInstance instance = whileLoopStack.Peek();
            // Check to see if we are in the current while loop....
            if (instance.StartLocation <= currentLocation && instance.EndLocation >= currentLocation)
            {
                // OK, we are in the while loop so we need to set the execution flow
                // to the end of the end of the while loop and also pop the whileLoopStack
                currentScriptLine = scriptArray[instance.EndLocation];
                whileLoopStack.Pop();
            }
        }

        /// <summary>
        /// This function handles executing the 'continue' keyword in the Funct script.
        /// </summary>
        public void ExecuteContinueStatement()
        {
            if (whileLoopStack.Count == 0) return;
            int currentLocation = currentScriptLine.LineNumber;
            WhileLoopInstance instance = whileLoopStack.Peek();
            // Check to see if we are in the current while loop....
            if (instance.StartLocation <= currentLocation && instance.EndLocation >= currentLocation)
            {
                // OK, we are in the while loop so we need to set the execution flow
                // to the top of the while loop, but we don't pop the stack
                currentScriptLine = scriptArray[instance.StartLocation - 1]; // We actually need to set it to -1 so that we execute the evaluation again...
            }


        }

        public WhileLoopInstance SearchWhileLoopListForMatchingStartLocation(ScriptLine sl)
        {

            foreach (WhileLoopInstance wli in whileLoopList)
            {
                if (wli.StartLocation == sl.LineNumber)
                {
                    return wli;
                }
            }
            // OK, If we made it here, it meant we never found an apppropriate while loop so something is major screwed up.
            throw new GeneralParseException("Failed To Find a matching While Loop Instance for line [" + sl.OriginalText + "]");
        }

        public ScriptLine FindCallLocation(string locationName)
        {
            foreach (FunctionInstance fi in functionList)
            {
                if (fi.FunctionName == locationName)
                {
                    return fi.FunctionScriptLine;
                }
            }
            throw new GeneralParseException("Failed To Locate Function [" + locationName + "] in the Funct script.");
        }

        public ScriptLine FindGotoLocation(string locationName)
        {
            foreach (ScriptLine sl in labelLocations)
            {
                if (sl.Text.Trim() == "Label " + locationName.ToString().Trim() + ":")
                {
                    return sl;
                }
            }
            throw new GeneralParseException("Failed To Locate Label [" + locationName + "] in the Funct script.");

        }

        /// <summary>
        /// This function executes the current script command if it's a Funct
        /// </summary>
        /// <returns>Returns a Funct result with the information returned by the Funct script.</returns>
        public FunctResult ExecuteCurrentFunct()
        {
            FunctResult tr = new FunctResult();
            // First We Need To Load The Parameters
            try
            {
                bool result = LoadParameters();
            }
            catch (ArgumentParseException ex)
            {
                tr.ErrorMessage = ex.Message;
                tr.Status = FunctStatus.Fail;
                this.scriptStack.Clear(); // This will clear the stack if something goes wrong...
                return tr;
            }

            // Now We Need To Find The Type of Funct To Run...

            Funct thisFunct;
            if (currentScriptLine.FunctFunctionName == null)
            {
                int dan = 0;
                dan++;
            }
            thisFunct = this.mapList.GetFunct(currentScriptLine.FunctFunctionName, this.variableCollection);


            if (thisFunct == null)
            {
                tr.ErrorMessage = "The Specified Funct To Run Could Not Be Associated With A Library Function.";
                tr.Status = FunctStatus.Fail;
                return tr;
            }
            // If We Made it here, we actually have the Funct, so we execute it and get the results.
            try
            {
                tr = thisFunct.Execute();
            }
            catch (VariableReadException ex)
            {
                tr.ErrorMessage = ex.Message;
                tr.Status = FunctStatus.Fail;
                return tr;
            }
            catch (System.MissingFieldException ex)
            {
                tr.ErrorMessage = ex.Message;
                tr.Status = FunctStatus.Fail;
                return tr;

            }
            catch (System.Exception ex)
            {
                tr.ErrorMessage = ex.Message;
                tr.Status = FunctStatus.Fail;
                return tr;
            }


            // Now Let's Put The Return Values Where They need to go...
            try
            {
                LoadReturnValues();
            }
            catch (ReturnValueParseException ex)
            {
                tr.ErrorMessage = ex.Message;
                tr.Status = FunctStatus.Fail;
                return tr;
            }
            // If We Failed, let's Go Ahead And Run The Cleanup Function
            // if it's set...

            return tr;
        }



        /// <summary>
        /// Loads the parameters for a Funct script command
        /// </summary>
        /// <returns>Parsing Status</returns>
        private bool LoadParameters()
        {
            // OK, Lets look at the current script line
            string temp = currentScriptLine.Text;


            // OK, in the new version of the engine, it's much simpler to load the
            // parameters... we just need to copy anything that's not a variable,
            // and if it is a variable, we pull it from the script, and make a copy
            // of it with the appropriate argument name.
            for (int x = 0; x < currentScriptLine.Arguments.Count; x++)
            {
                if (currentScriptLine.Arguments["Argument" + x.ToString()].variableType != VariableType.Variable)
                {
                    variableCollection.SetArgument(x, currentScriptLine.Arguments["Argument" + x.ToString()]);
                }
                else
                {
                    try
                    {
                        FunctVariable sv = variableCollection.getVariable(currentScriptLine.Arguments["Argument" + x.ToString()].Variable.ToString());
                        variableCollection.SetArgument(x, sv);
                    }
                    catch (KeyNotFoundException)
                    {
                        throw new ArgumentParseException("Variable Could Not Be Loaded: [" + currentScriptLine.Arguments["Argument" + x.ToString()].Variable.ToString() + "]");
                    }
                }
            }

            FunctVariable argCount = new FunctVariable("ToTalArguments", VariableType.Integer, currentScriptLine.Arguments.Count);
            variableCollection.setVariable(argCount);

            return true;
        }

        private bool ParseFunctionArguments(string scriptLineText, Dictionary<string, FunctVariable> arguments)
        {
            /// <summary>
            /// Loads the parameters for a Funct script command
            /// </summary>
            /// <returns>Parsing Status</returns>
            // OK, Lets look at the current script line
            try
            {
                string temp;
                temp = scriptLineText;

                // First, if we have an equals, let's throw everything to the left of it away...

                if (RemoveQuotedCharacters(temp).IndexOf("=") != -1)
                {
                    temp = temp.Substring(temp.IndexOf("="));
                    temp.Trim();
                }


                // OK, Now Let's Throw away everything to the left of the '('
                if (temp.IndexOf("(") != -1)
                {
                    temp = temp.Substring(temp.IndexOf("(") + 1);
                    temp.Trim();
                }

                //OK, Now Let's Throw away everything to the right of the ')'
                if (temp.IndexOf(")") != -1)
                {
                    temp = temp.Substring(0, temp.LastIndexOf(")"));
                    temp.Trim();
                }


                // Ok, If the Funct had no parameters, this should be blank, so let's
                // just return.
                if (temp.Length == 0) return true;

                // Now, if it wasn't blank, we have a friggin' parameter list here!
                // DOH!

                //Let's find out how many parameters we have
                int x;
                bool inquote = false;
                int paramcount = 1;
                string temp2 = RemoveQuotedCharacters(temp);
                foreach (char c in temp2)
                {
                    if (c == ',') paramcount++;
                }

                string[] parameters = new string[paramcount];

                // OK, We Now Have The Accurate Parameter Count... Now We Need To Load The Paramters...
                int y = 0;
                inquote = false;
                int z = 0;
                x = 0;
                while (x < paramcount)
                {
                    try
                    {
                        if (y > temp.Length - 1) break;
                        if (temp[y] == '"')
                        {
                            if (inquote == true) inquote = false;
                            else inquote = true;
                        }
                        if ((inquote == false) && (temp[y] == ','))
                        {
                            x++;
                            z = 0;
                        }
                        else
                        {
                            parameters[x] = parameters[x] + temp[y];
                            z++;
                        }
                        y++;
                    }
                    catch (System.IndexOutOfRangeException)
                    {
                        break;
                    }
                }

                // OK, Now We Need To Figure Out How To Handle Each parameter
                // Each parameter is either an variable, or a literal.
                // Literals can be strings, floats, and ints

                int paramindex = 0;
                foreach (string param in parameters)
                {
                    string temp_param = param.Trim();
                    if (ParameterIsString(temp_param))
                    {
                        FunctVariable scriptVar;
                        bool worked = CreaFunctringVariable(temp_param, paramindex, out scriptVar);
                        if (worked == false) return false;
                        arguments.Add(scriptVar.Name, scriptVar);
                    }
                    else if (ParameterIsInt(temp_param))
                    {
                        FunctVariable scriptVar;
                        CreateIntegerVariable(temp_param, paramindex, out scriptVar);
                        arguments.Add(scriptVar.Name, scriptVar);

                    }
                    else if (ParameterIsFloat(temp_param))
                    {
                        FunctVariable scriptVar;
                        CreateFloatVariable(temp_param, paramindex, out scriptVar);
                        arguments.Add(scriptVar.Name, scriptVar);
                    }
                    else if (ParameterIsBoolean(temp_param))
                    {
                        FunctVariable scriptVar;
                        CreateBooleanVariable(temp_param, paramindex, out scriptVar);
                        arguments.Add(scriptVar.Name, scriptVar);
                    }

                    else if (ParameterIsVariable(temp_param))
                    {
                        FunctVariable scriptVar;
                        CreateVariableHoldingVariable(temp_param, paramindex, out scriptVar);
                        arguments.Add(scriptVar.Name, scriptVar);
                    }
                    else
                    {
                        return false;
                    }
                    paramindex++; // Update the parameter index.
                }
                return true;
            }
            catch (NullReferenceException)
            {
                return false;
            }


        }

        /// <summary>
        /// Loads the return values for a Funct script command
        /// </summary>
        /// <returns>Parsing Status</returns>
        private bool LoadReturnValues()
        {
            if (currentScriptLine.OutVariableNames != null)
            {

                for (int x = 0; x < currentScriptLine.OutVariableNames.Count; x++)
                {
                    bool result = CopyOutValueToScriptVariable(currentScriptLine.OutVariableNames[x], x);
                }
                return true;
            }
            else
            {
                return true;
            }
        }

        private bool ParseReturnValues(string scriptLineText, out List<string> returnVarNames)
        {

            returnVarNames = new List<string>();

            // OK, Lets look at the current script line
            string temp = scriptLineText;
            temp = temp.Trim();

            // First, if we don't have an equals, let's just return and call it a day...
            if (RemoveQuotedCharacters(temp).IndexOf("=") == -1)
            {
                return true;
            }

            // If We Are Here, We Must have an Equals... so let's throw away
            // everything to the RIGHT of it.
            temp = RemoveQuotedCharacters(temp);
            temp = temp.Substring(0, temp.IndexOf("="));
            temp.Trim();

            // Ok, Now We Should Just Have The Return list...
            // This May, or may not have parantheses around it.
            if (temp.IndexOf("(") != -1)
            {
                // We Have a Parantheses, let's trim it and everything to the left.
                temp = temp.Substring(temp.IndexOf("(") + 1);
                temp.Trim();
            }

            //OK, Now Let's Throw away everything to the right of the ')' if it exists
            if (temp.IndexOf(")") != -1)
            {
                temp = temp.Substring(0, temp.IndexOf(")"));
                temp.Trim();
            }

            // Ok, if We Made it here... we should just have the return list.
            if (temp.Length == 0) return true; // If Funct Had No Returns Just exit.

            // Now, if it wasn't blank, we have a friggin' return list here!
            // DOH!

            //Let's find out how many parameters we have
            int x;
            int paramcount = 1;
            for (x = 0; x < temp.Length; x++)
            {
                if (temp[x] == ',') { paramcount++; }
            }

            string[] parameters = new string[paramcount];
            parameters = temp.Split(new char[] { ',' }, paramcount);

            // OK, Now We Need To Figure Out How To Handle Each parameter
            // Each parameter is either an variable, or a literal.
            // Literals can be strings, floats, and ints

            foreach (string param in parameters)
            {
                returnVarNames.Add(param.Trim());
            }
            return true; ;
        }


        /// <summary>
        /// This function removes any comments from the Funct script
        /// line by starting at the back and walking forward.
        /// 
        /// Includes a sanity check for short-circuit exit for improved performance
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
		public static string RemoveCommentFromLine(string line)
        {
            // If we don't have a '//' string then let's just return now...
            if (line.IndexOf("//") == -1) return line.Trim();

            // OK, we need to walk through each thing and if we ever find a '//' that's not
            // in a quoted section of string, we need to return everything in the string
            // up to that point.
            Ader.Text.StringTokenizer tokenizer = new Ader.Text.StringTokenizer(line);
            Ader.Text.Token token;
            StringBuilder retString = new StringBuilder();
            do
            {
                token = tokenizer.Next();

                if (token.Value == "/")
                {
                    // OK, we may actually be at the start of a comment, so we need to peek at the next character
                    Ader.Text.Token token2 = tokenizer.Next();
                    if (token2.Value == "/")
                    {
                        // OK, if we made in this loop, we just found the comment, so we
                        // break out right now
                        break;
                    }
                    else
                    {
                        // OK, it wasn't another '/' mark so let's add both the tokens and keep on trucking...
                        retString.Append(token.Value);
                        retString.Append(token2.Value);
                    }
                }
                else
                {
                    // Add the token and keep looping...
                    retString.Append(token.Value);
                }

            } while (token.Kind != Ader.Text.TokenKind.EOF);

            return retString.ToString().Trim();

        }

        /// <summary>
        /// Determines if a parameter is a string
        /// </summary>
        /// <param name="param">The extracted parameter</param>
        /// <returns></returns>
        /// 

        private static bool ParameterIsString(string param)
        {
            if ((param[0] == '"') && (param[param.Length - 1] == '"'))
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        /// <summary>
        /// Determines if a parameter is an Integer
        /// </summary>
        /// <param name="param">The extracted parameter</param>
        /// <returns></returns>
        private static bool ParameterIsInt(string param)
        {

            long result;
            if (long.TryParse(param, System.Globalization.NumberStyles.Integer, null, out result) == false)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Determines if a parameter is a float
        /// </summary>
        /// <param name="param">The extracted parameter</param>
        /// <returns></returns>
        private static bool ParameterIsFloat(string param)
        {
            double result;
            if (double.TryParse(param, System.Globalization.NumberStyles.Float, null, out result) == false)
            {
                return false;
            }
            return true;

        }

        private static bool ParameterIsBoolean(string param)
        {
            bool result;
            if (bool.TryParse(param, out result) == false)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Determines if a parameter is a variable
        /// </summary>
        /// <param name="param">The extracted parameter</param>
        /// <returns></returns>
        private static bool ParameterIsVariable(string param)
        {
            if (param[0] != '$') { return false; }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Loads a string into a variable
        /// </summary>
        /// <param name="param">The extracted parameter</param>
        /// <param name="paramindex">The index of the parameter</param>
        /// <returns></returns>
        private bool LoadStringIntoVariable(string param, int paramindex)
        {
            FunctVariable newvar = new FunctVariable();
            newvar.Name = "Argument" + paramindex.ToString();
            newvar.variableType = VariableType.String;
            param = param.Trim();
            param = param.Substring(1);
            param = param.Substring(0, param.Length - 1);

            // 3/25/2004 - Added Code to Translate
            // Escape characters into their control character
            // equivalents, this will allow far more powerful 
            // scripting potential for string literals...
            try
            {
                param = StringLiteralEscapeCodeParser.ParseEscapeCodes(param);
            }
            catch (GeneralParseException)
            {
                return false;

            }

            // End of Code Additions 3/25/2004

            newvar.Variable = param;
            this.variableCollection.setVariable(newvar);
            return true;
        }

        private static bool CreaFunctringVariable(string param, int paramIndex, out FunctVariable retVar)
        {
            FunctVariable newvar = new FunctVariable();
            newvar.Name = "Argument" + paramIndex.ToString();
            newvar.variableType = VariableType.String;
            param = param.Trim();
            param = param.Substring(1);
            param = param.Substring(0, param.Length - 1);

            // 3/25/2004 - Added Code to Translate
            // Escape characters into their control character
            // equivalents, this will allow far more powerful 
            // scripting potential for string literals...
            try
            {
                param = StringLiteralEscapeCodeParser.ParseEscapeCodes(param);
            }
            catch (GeneralParseException)
            {
                retVar = newvar;
                return false;

            }

            // End of Code Additions 3/25/2004

            newvar.Variable = param;
            retVar = newvar;
            return true;
        }



        private static void CreateIntegerVariable(string param, int paramindex, out FunctVariable retVar)
        {
            FunctVariable newvar = new FunctVariable();
            newvar.Name = "Argument" + paramindex.ToString();
            newvar.variableType = VariableType.Integer;
            try
            {
                newvar.Variable = int.Parse(param);
            }
            catch (System.OverflowException)
            {
                newvar.Variable = long.Parse(param);
            }
            retVar = newvar;
            return;
        }




        private static void CreateFloatVariable(string param, int paramindex, out FunctVariable retVar)
        {
            FunctVariable newvar = new FunctVariable();
            newvar.Name = "Argument" + paramindex.ToString();
            newvar.variableType = VariableType.Float;
            newvar.Variable = double.Parse(param);
            retVar = newvar;
            return;
        }

        private static void CreateBooleanVariable(string param, int paramindex, out FunctVariable retVar)
        {
            FunctVariable newvar = new FunctVariable();
            newvar.Name = "Argument" + paramindex.ToString();
            newvar.variableType = VariableType.Boolean;
            newvar.Variable = bool.Parse(param);
            retVar = newvar;
            return;
        }



        private static void CreateVariableHoldingVariable(string param, int paramindex, out FunctVariable retVar)
        {
            FunctVariable newvar = new FunctVariable();
            retVar = newvar;

            // Now We Need To Get The Variable That We Want.
            // If It Doesn't Exist, we need to return false.

            //strip the dollar sign from the front.
            param = param.Substring(1);
            retVar.variableType = VariableType.Variable;
            retVar.Variable = param;
            retVar.Name = "Argument" + paramindex.ToString();
            return;
        }



        /// <summary>
        /// Copies all the return values into the return value list
        /// </summary>
        /// <param name="param">The extracted parameter</param>
        /// <param name="paramindex">The index of the parameter</param>
        /// <returns></returns>
        private bool CopyOutValueToScriptVariable(string param, int paramindex)
        {
            FunctVariable newvar = new FunctVariable();

            //strip the dollar sign from the front.
            param = param.Substring(1);
            param = param.Trim(); // Remove any whitespace around the characters

            // Now We Need To Get The Variable That We Want.
            // If It Doesn't Exist, we need to return false.
            try
            {
                newvar = this.variableCollection.getVariable("ReturnValue" + paramindex.ToString());
            }
            catch (System.Collections.Generic.KeyNotFoundException)
            {
                throw (new ReturnValueParseException("Line [" + curScriptLine.ToString() + "]: The Return Value For Argument" + paramindex.ToString() + " Could Not Be Found in the Variable Space"));
            }
            FunctVariable returnvar = new FunctVariable();
            returnvar.Name = param;
            returnvar.variableType = newvar.variableType;
            returnvar.Variable = newvar.Variable;

            // Now Let's Set The Variable
            variableCollection.setVariable(returnvar);
            return true;
        }

        /// <summary>
        /// Returns true or false indicating whether or not the Funct script has
        /// an Finalize function
        /// </summary>
        public bool HasFinalizeFunction
        {
            get
            {
                return variableCollection.VariableExists("RESERVED_FinalizeFunction");
            }
        }
        /// <summary>
        /// Returns true or false indicating whether or not the Funct script has a fail
        /// function.
        /// </summary>
        public bool HasFailFunction
        {
            get
            {
                return variableCollection.VariableExists("RESERVED_FailFunction");
            }
        }

        /// <summary>
        /// Returns true or false indicating whether or not the Funct script has an abort
        /// function.
        /// </summary>
        public bool HasAbortFunction
        {
            get
            {
                return variableCollection.VariableExists("RESERVED_AbortFunction");
            }
        }

        /// <summary>
        /// This function Removes All The Quoted Characters From A String
        /// Before Determining the type of function that it is...
        /// For instance, if an '=' sign exists inside a quoted string
        /// it's removed to prevent it from confusing the parser.
        /// </summary>
        /// <param name="temp">The temporary string to parse</param>
        /// <returns>The parsed string</returns>
        private static string RemoveQuotedCharacters(string temp)
        {
            StringBuilder myString = new StringBuilder();
            Ader.Text.StringTokenizer tokenizer = new Ader.Text.StringTokenizer(temp);
            Ader.Text.Token myToken;
            myToken = tokenizer.Next();
            while (myToken.Kind != Ader.Text.TokenKind.EOF)
            {
                if (myToken.Kind != Ader.Text.TokenKind.QuotedString)
                {
                    myString.Append(myToken.Value);
                }
                myToken = tokenizer.Next();
            }
            return myString.ToString();
        }


        /// <summary>
        /// Runs the abort function of the Funct script.
        /// </summary>
        public List<FunctResult> RunAbortFunction()
        {

            // Find if finalize function is set.
            FunctVariable temp = new FunctVariable();
            if (variableCollection.VariableExists("RESERVED_AbortFunction"))
            {
                temp = variableCollection.getVariable("RESERVED_AbortFunction");
                string abortfuncname = temp.Variable.ToString();
                ScriptLine sl = new ScriptLine();
                sl.Text = "call " + abortfuncname + "();";
                sl.OriginalText = "call " + abortfuncname + "();";
                sl.ScriptLineType = ScriptLineType.Call;
                currentScriptLine = sl;
            }
            else
            {
                return new List<FunctResult>(); ;
            }

            // Here We Need To perform whatever is required for finalization...
            this.JumpToCall();
            this.Status = ScriptStatus.Running;
            List<FunctResult> resultList = new List<FunctResult>();

            while (this.Status != ScriptStatus.Finished)
            {
                try
                {
                    this.GetNextFunct();
                }

                catch (GeneralParseException ex)
                {
                    Console.WriteLine(ex.Message);
                    break;
                }

                if (this.Status == ScriptStatus.Finished)
                {
                    break;
                }
                if (this.Status == ScriptStatus.Error)
                {
                    break;
                }

                FunctResult tr = this.ExecuteCurrentFunct();
                tr.ErrorMessage = "ABORT FUNCTION";
                resultList.Add(tr);

            }
            return resultList;
        }

        /// <summary>
        /// Runs the finalize function of the Funct script.
        /// </summary>
        public List<FunctResult> RunFinalizeFunction()
        {

            // Find if finalize function is set.
            FunctVariable temp = new FunctVariable();
            if (variableCollection.VariableExists("RESERVED_FinalizeFunction"))
            {
                temp = variableCollection.getVariable("RESERVED_FinalizeFunction");
                string finalfuncname = temp.Variable.ToString();
                ScriptLine sl = new ScriptLine();
                sl.Text = "call " + finalfuncname + "();";
                sl.OriginalText = "call " + finalfuncname + "();";
                sl.ScriptLineType = ScriptLineType.Call;
                currentScriptLine = sl;
            }
            else
            {
                return new List<FunctResult>(); ;
            }

            // Here We Need To perform whatever is required for finalization...
            this.JumpToCall();
            this.Status = ScriptStatus.Running;
            List<FunctResult> resultList = new List<FunctResult>();

            while (this.Status != ScriptStatus.Finished)
            {


                try
                {
                    this.GetNextFunct();
                }

                catch (GeneralParseException ex)
                {
                    Console.WriteLine(ex.Message);
                    break;
                }

                if (this.Status == ScriptStatus.Finished)
                {
                    break;
                }
                if (this.Status == ScriptStatus.Error)
                {
                    break;
                }

                FunctResult tr = this.ExecuteCurrentFunct();
                tr.ErrorMessage = "FINALIZE FUNCTION";
                resultList.Add(tr);

            }

            return resultList;




        }

        /// <summary>
        /// Runs the fail function of the Funct script.
        /// </summary>
        public List<FunctResult> RunFailFunction()
        {

            // Find if finalize function is set.
            FunctVariable temp = new FunctVariable();
            if (variableCollection.VariableExists("RESERVED_FailFunction"))
            {
                temp = variableCollection.getVariable("RESERVED_FailFunction");
                string finalfuncname = temp.Variable.ToString();
                ScriptLine sl = new ScriptLine();
                sl.Text = "call " + finalfuncname + "();"; ;
                sl.OriginalText = "call " + finalfuncname + "();"; ;
                sl.ScriptLineType = ScriptLineType.Call;
                currentScriptLine = sl;
            }
            else
            {
                return new List<FunctResult>();
            }

            // Here We Need To perform whatever is required for finalization...
            try
            {
                this.JumpToCall();
            }
            catch (GeneralParseException)
            {
                return new List<FunctResult>();
            }
            this.Status = ScriptStatus.Running;
            List<FunctResult> resultList = new List<FunctResult>();
            while (this.Status != ScriptStatus.Finished)
            {
                try
                {
                    this.GetNextFunct();
                }

                catch (GeneralParseException ex)
                {
                    Console.WriteLine(ex.Message);
                    break;
                }

                if (this.Status == ScriptStatus.Finished)
                {
                    break;
                }
                if (this.Status == ScriptStatus.Error)
                {
                    break;
                }

                FunctResult tr = this.ExecuteCurrentFunct();
                tr.ErrorMessage = "FAIL FUNCTION";
                resultList.Add(tr);



            }
            return resultList;







        }



        /*
		 * This function finds all the functions inside a Funct script and returns an array of the
		 * function names.  This is used for the debug screen.
		 */
        public ArrayList getFunctions()
        {
            ArrayList functionArray = new ArrayList();
            foreach (FunctionInstance fi in this.functionList)
            {
                functionArray.Add(fi.FunctionName);
            }
            return functionArray;
        }

        /// <summary>
        /// Runs a function by name (used for the debug mode)
        /// </summary>
        /// <param name="functionName">The function name to execute</param>
        /// <param name="stopOnFail">Whether or not to stop on failure</param>
        /// <returns>An ArrayList containing the Functresult from each executed function.</returns>
        public List<FunctResult> RunDebugFunction(string functionName, bool stopOnFail)
        {
            this.scriptStack.Clear();

            // Find if finalize function is set.
            FunctVariable temp = new FunctVariable();
            ScriptLine sl = new ScriptLine();
            sl.ScriptLineType = ScriptLineType.Call;
            sl.OriginalText = "call " + functionName + ";";
            sl.Text = "call " + functionName + ";";
            currentScriptLine = sl;

            // Here We Need To perform whatever is required for finalization...
            this.JumpToDebugFunction();
            this.Status = ScriptStatus.Running;
            List<FunctResult> FunctResults = new List<FunctResult>();
            while (this.Status != ScriptStatus.Finished)
            {
                try
                {
                    this.GetNextFunct();
                }

                catch (GeneralParseException ex)
                {
                    this.scriptStack.Clear();
                    FunctResult ts = new FunctResult();
                    ts.Status = FunctStatus.Fail;
                    ts.FunctName = "Script Engine Exception";
                    ts.ErrorMessage = "Script Engine Exception";
                    FunctResults.Add(ts);
                    Console.WriteLine(ex.Message);
                    return FunctResults;
                }

                if (this.Status == ScriptStatus.Finished)
                {
                    this.scriptStack.Clear();
                    return FunctResults;
                }
                if (this.Status == ScriptStatus.Error)
                {
                    this.scriptStack.Clear();
                    return FunctResults;
                }

                FunctResult tr = this.ExecuteCurrentFunct();
                FunctResults.Add(tr);
                if ((tr.Status == FunctStatus.Fail) && (stopOnFail == true))
                {
                    this.scriptStack.Clear();
                    return FunctResults;
                }
            }
            this.scriptStack.Clear();
            return FunctResults;
        }

        public bool Compile(ref List<CompilerError> compilerErrorsOut)
        {

            foreach (ScriptLine sl in scriptArray)
            {
                sl.ScriptLineType = AnalyzeScriptLine(sl, mapList);

                if (sl.ScriptLineType == ScriptLineType.Unknown)
                {
                    CompilerError compilerError = new CompilerError("Unknown Command Type", sl);
                    compilerErrors.Add(compilerError);
                }
                if (sl.ScriptLineType == ScriptLineType.Funct || sl.ScriptLineType == ScriptLineType.ScriptIf)
                {
                    CompileFunctLine(sl);
                }

            }

            CheckForDuplicateLabels(ref compilerErrors);
            CheckForDuplicateFunctions(ref compilerErrors);
            CheckThatAllIfStatementsHaveOpeningBlock(ref compilerErrors);
            CheckThatAllWhileLoopStatementsHaveOpeningBlock(ref compilerErrors);
            CheckThatAllOpeningBracketsHaveClosingBrackets(ref compilerErrors);
            CheckThatAllCallStatementsPointToValidFunctions(ref compilerErrors);
            CheckThatAllGotoStatementsPointToValidLabels(ref compilerErrors);

            compilerErrorsOut = this.compilerErrors;
            if (compilerErrors.Count == 0)
            {
                // OK, If we didn't find any errors in the file, we want to build a list of all the while loops...
                BuildWhileLoopList();
                SetClosingBlockOfFunctions();
                return true;
            }
            else return false;


        }

        /// <summary>
        /// This function is going to create a line of all the while loops starting and ending brackets....
        /// </summary>
        private void BuildWhileLoopList()
        {
            foreach (ScriptLine sl in scriptArray)
            {
                sl.ScriptLineType = AnalyzeScriptLine(sl, mapList);

                if (sl.ScriptLineType == ScriptLineType.WhileLoop)
                {
                    WhileLoopInstance instance = new WhileLoopInstance();
                    instance.StartLocation = sl.LineNumber;
                    instance.EndLocation = FindCodeBlockEndLocation(sl);
                    whileLoopList.Add(instance);
                }
            }
        }

        /// <summary>
        /// This is an internal function used for finding the end location of a given while loop....
        /// </summary>
        /// <returns></returns>
        private int FindCodeBlockEndLocation(ScriptLine sl)
        {


            // Actually, this is a bit more complicated:(  We need to supported
            // nested while loop statements which means that we need to increment
            // for each opening bracket statement contained in the block...


            int loc = sl.LineNumber;
            int levelCount = 0; // This is the current cyclometric depth

            try
            {
                while ((scriptArray[loc].ScriptLineType != ScriptLineType.RightBracket) || (levelCount != 1))
                {
                    if (scriptArray[loc].ScriptLineType == ScriptLineType.LeftBracket)
                    {
                        levelCount++;
                    }
                    if (scriptArray[loc].ScriptLineType == ScriptLineType.RightBracket)
                    {
                        levelCount--;
                    }
                    loc++;
                }
                return loc;
            }
            catch (ArgumentOutOfRangeException)
            {
                throw new GeneralParseException("The Closing Brackret of the Code Block At Statement [" + sl.OriginalText + "]  Could Not Be Found");
            }

        }

        public void CheckForDuplicateFunctions(ref List<CompilerError> compilerErrors)
        {
            foreach (FunctionInstance f1 in functionList)
            {
                int count = 0;
                foreach (FunctionInstance f2 in functionList)
                {
                    if (f1.FunctionName == f2.FunctionName)
                    {
                        count++;
                        if (count > 1)
                        {
                            compilerErrors.Add(new CompilerError("Duplicate Functions Found [" + f1.FunctionName + "]", f1.FunctionScriptLine));
                        }
                    }
                }

            }

        }

        public void CheckThatAllIfStatementsHaveOpeningBlock(ref List<CompilerError> compilerErrors)
        {
            for (int x = 0; x <= scriptArray.Count - 1; x++)
            {
                if (scriptArray[x].ScriptLineType == ScriptLineType.If)
                {
                    if ((x + 1) <= (scriptArray.Count - 1))
                    {
                        if (scriptArray[x + 1].ScriptLineType != ScriptLineType.LeftBracket)
                        {
                            compilerErrors.Add(new CompilerError("The If Statement is Missing an Opening Left Bracket", scriptArray[x]));
                        }
                    }
                }

            }
        }

        public void CheckThatAllWhileLoopStatementsHaveOpeningBlock(ref List<CompilerError> compilerErrors)
        {
            for (int x = 0; x <= scriptArray.Count - 1; x++)
            {
                if (scriptArray[x].ScriptLineType == ScriptLineType.WhileLoop)
                {
                    if ((x + 1) <= (scriptArray.Count - 1))
                    {
                        if (scriptArray[x + 1].ScriptLineType != ScriptLineType.LeftBracket)
                        {
                            compilerErrors.Add(new CompilerError("The While Loop Statement is Missing an Opening Left Bracket", scriptArray[x]));
                        }
                    }
                }

            }
        }

        public void CheckThatAllOpeningBracketsHaveClosingBrackets(ref List<CompilerError> compilerErrors)
        {
            int counter = 0;

            for (int x = 0; x <= scriptArray.Count - 1; x++)
            {

                if (scriptArray[x].ScriptLineType == ScriptLineType.LeftBracket)
                {
                    counter++;
                }
                if (scriptArray[x].ScriptLineType == ScriptLineType.RightBracket)
                {
                    counter--;
                }
                if (counter < 0)
                {
                    compilerErrors.Add(new CompilerError("The Closing Bracket does not have a corresponding opening bracket.", scriptArray[x]));
                    return;
                }
            }
        }

        public void CheckForDuplicateLabels(ref List<CompilerError> compilerErrors)
        {
            foreach (ScriptLine l1 in labelLocations)
            {
                int count = 0;
                foreach (ScriptLine l2 in labelLocations)
                {
                    if (l1.Text.Trim() == l2.Text.Trim())
                    {
                        count++;
                        if (count > 1)
                        {
                            compilerErrors.Add(new CompilerError("Duplicate Label Found [" + l2.Text.Replace("goto", "").Trim().Replace(";", "").ToString() + "]", l1));
                        }
                    }
                }
            }
        }

        public void CheckThatAllCallStatementsPointToValidFunctions(ref List<CompilerError> compilerErrors)
        {
            foreach (ScriptLine l1 in scriptArray)
            {
                if (l1.ScriptLineType == ScriptLineType.Call)
                {
                    string functionNameCalled = l1.Text.Trim().Substring(4, l1.Text.Trim().Length - 4);
                    functionNameCalled = functionNameCalled.Replace(";", "");
                    functionNameCalled = functionNameCalled.Replace("()", "").Trim();


                    bool found = false;
                    foreach (FunctionInstance fi in functionList)
                    {
                        if (fi.FunctionName == functionNameCalled)
                        {
                            found = true;
                            break;
                        }
                    }
                    if (found == false)
                    {
                        compilerErrors.Add(new CompilerError("Call Statement References a Function Which Does Not Exist.", l1));
                    }
                }
            }
        }

        public void CheckThatAllGotoStatementsPointToValidLabels(ref List<CompilerError> compilerErrors)
        {
            foreach (ScriptLine l1 in scriptArray)
            {
                if (l1.ScriptLineType == ScriptLineType.Goto)
                {
                    string gotoLocationCalled = l1.Text.Trim().Substring(4, l1.Text.Trim().Length - 4).Replace(";", "");


                    bool found = false;
                    foreach (ScriptLine l2 in labelLocations)
                    {
                        string labelLocationName = l2.Text.Trim().Substring(5, l2.Text.Trim().Length - 5).Replace(":", "");
                        if (labelLocationName == gotoLocationCalled)
                        {
                            found = true;
                            break;
                        }
                    }
                    if (found == false)
                    {
                        compilerErrors.Add(new CompilerError("Goto Statement References a Label Which Does Not Exist.", l1));
                    }
                }
            }
        }

        /// <summary>
        /// This function is called from the Compile function, and it 
        /// handles building the ScriptLine from the raw text for Funct Functions
        /// </summary>
        /// <param name="sl"></param>
        private void CompileFunctLine(ScriptLine sl)
        {
            bool worked = ParseFunctionArguments(sl.Text, sl.Arguments);
            if (worked == false)
            {
                CompilerError compilerError = new CompilerError("Failed To Parse Function Arguments", sl);
                compilerErrors.Add(compilerError);
            }
            List<string> outVariableNames;
            worked = ParseReturnValues(sl.Text, out outVariableNames);
            if (worked == true)
            {
                sl.OutVariableNames = outVariableNames;
            }
            else
            {
                CompilerError compilerError = new CompilerError("Failed To Parse Function Return Values", sl);
                compilerErrors.Add(compilerError);
            }
        }

        public ScriptLine GetCurrentScriptLine()
        {
            return currentScriptLine;
        }

        public List<ScriptLine> GetCurrentFunctScript()
        {
            return scriptArray;
        }

        public VariableCollection VariableCollection
        {
            get
            {
                return this.variableCollection;
            }
        }

        /// <summary>
        /// This function redirects a currently executing Funct script to a given 
        /// label location.
        /// </summary>
        /// <param name="labelLocation">The name of the label location.</param>
        /// <returns></returns>
        public bool JumpToLabelLocation(string labelLocation)
        {
            try
            {
                ScriptLine sl = this.FindGotoLocation(labelLocation);
                this.currentScriptLine = sl;
                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("The Following Error Occurred [" + ex.Message + "]");
            }

        }


    }
}

