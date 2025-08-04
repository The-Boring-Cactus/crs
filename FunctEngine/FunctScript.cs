using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using FunctEngine.Enums;
using FunctEngine.Exceptions;

namespace FunctEngine
{
    public class FunctScript
    {
        private FunctResults Funct_results = new FunctResults();
        private string scriptName = string.Empty;
        public FunctScriptInterpreter interpreter;
        public FunctList mappingList;
        public List<FunctResult> tempFunctResults = new List<FunctResult>();
        private StatementEvaluator statementEvaluator;
        private VariableCollection varCollection;
        private string SelectedFunctScript = string.Empty;
        public List<string> compileErrors = new List<string>();
        private FunctStatus Funct_status = FunctStatus.NotRun;
        bool recordElapsedTimes = true;
        double elapsedFunctTime = 0;
        public event StatusUpdateHandler StatusUpdate;
        private GlobalVariables globalVariables;
        public event StatusUpdateHandler StatusFinish;
        public bool abortRequested = false;
        
        public FunctScript(FunctList mapList, StatementEvaluator statementEvaluator,  GlobalVariables _globalVariables)
        {
            globalVariables = _globalVariables;
            varCollection = new VariableCollection(globalVariables);
            mappingList = mapList;
            Funct_status = FunctStatus.NotRun;
            
            
            this.statementEvaluator = statementEvaluator;
            varCollection.StatusUpdate += statusUpdate_proc;
            
            
        }

        

        private void statusUpdate_proc(object sender, StatusString e)
        {
            if (this.StatusUpdate != null)
            {
                this.StatusUpdate(this, e);
            }
        }

        public bool InitializeScript(string SelectedFunctScript, string ScriptName, ref List<CompilerError> compilerErrors)
        {
            Funct_status = FunctStatus.NotRun;
            compileErrors = new List<string>();

            this.scriptName = ScriptName;
            this.SelectedFunctScript = SelectedFunctScript;
            interpreter = new FunctScriptInterpreter(this.SelectedFunctScript, varCollection, mappingList, statementEvaluator, scriptName);
            bool worked = true;
            worked = interpreter.InitializeScript(ref compilerErrors);
            if (worked == false)
            {
                foreach (CompilerError ce in compilerErrors)
                {
                    compileErrors.Add(ce.errorDescription);
                }
                return worked;
            }
            return worked;
        }

        public void RequestAbort() => abortRequested = true;
        public void StartFunct()
        {


            // Set the Thread Regionalization

            System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.CreateSpecificCulture("EN-US");


            // Set The StartTime For The Funct Results
            Funct_results.SetStartTime();

            FunctResult tstresult;


            DateTime startFunctTime = DateTime.Now;
            DateTime endFunctTime = DateTime.Now;
            TimeSpan elapsedTimeSpan = new TimeSpan();
            List<CompilerError> compilerErrors = new List<CompilerError>();

            Compile(ref compilerErrors);

            while ((interpreter.Status != ScriptStatus.Finished) && (interpreter.Status != ScriptStatus.Error) && (this.Funct_status != FunctStatus.Fail) && (this.Funct_status != FunctStatus.Abort))
            {
                if (abortRequested)
                {
                    this.Funct_status = FunctStatus.Abort;
                    this.Funct_results.status = FunctStatus.Abort;
                    this.Funct_results.SetStopTime();

                    break;
                }

                try
                {
                    interpreter.GetNextFunct();
                }
                catch (GeneralParseException ex)
                {

                    this.Funct_status = FunctStatus.Fail;
                    Funct_results.status = FunctStatus.Fail;
                    Funct_results.SetStopTime();
                    interpreter.Status = ScriptStatus.Error;
                    Funct_results.StatusText = "Fail";
                    Console.WriteLine(ex.Message);
                    break;
                }


                if (interpreter.Status == ScriptStatus.AtBreakpoint)
                {
                    break;

                }

                if (interpreter.Status == ScriptStatus.Finished)
                {
                    Funct_results.SetStopTime();
                    break;
                }
                if (interpreter.Status == ScriptStatus.Error)
                {
                    this.Funct_status = FunctStatus.Fail;
                    Funct_results.status = FunctStatus.Fail;
                    Funct_results.SetStopTime();
                    Funct_results.StatusText = "Error";
                }

                // If we Are set up to record the elapsed times
                // do the top if... else do the bottom
                if (this.recordElapsedTimes)
                {
                    startFunctTime = DateTime.Now;
                    tstresult = interpreter.ExecuteCurrentFunct();
                    endFunctTime = DateTime.Now;
                    elapsedTimeSpan = endFunctTime - startFunctTime;
                    tstresult.executionTime = elapsedTimeSpan.TotalSeconds;
                    elapsedFunctTime = elapsedFunctTime + elapsedTimeSpan.TotalSeconds;
                    tstresult.elapsedTime = elapsedFunctTime;
                }
                else
                {
                    if (interpreter.Status != ScriptStatus.Error)
                    {
                        tstresult = interpreter.ExecuteCurrentFunct();
                    }
                    else
                    {
                        tstresult = new FunctResult();
                    }
                }


                // Add The Funct Result To The Output

                Funct_results.AddResult(tstresult);
                tempFunctResults.Add(tstresult);





                if (tstresult.Status == FunctStatus.Fail)
                {
                    this.Funct_status = FunctStatus.Fail;
                    Funct_results.status = FunctStatus.Fail;
                    Funct_results.FailMessage = tstresult.ErrorMessage;
                    Funct_results.SetStopTime();
                    Funct_results.StatusText = "Fail";
                    break;
                }



                // If We Aborted, we should stop
                if (tstresult.Status == FunctStatus.Abort)
                {
                    this.Funct_status = FunctStatus.Abort;
                    Funct_results.status = FunctStatus.Abort;
                    Funct_results.SetStopTime();
                    Funct_results.StatusText = "Abort";
                    break;
                }


            } // End Of While Loop.


            // Now Let's copy the name of the script file.
            Funct_results.FunctScript = scriptName;

            
            // If not a failure or abort, then generate a pass.
            if ((Funct_results.status != FunctStatus.Fail) && (Funct_status != FunctStatus.Fail) && (Funct_status != FunctStatus.Abort))
            {
                Funct_results.SetStopTime();
                Funct_results.status = FunctStatus.Pass;
                Funct_status = FunctStatus.Pass;
                Funct_results.StatusText = "Pass";
            }
            else if (Funct_results.status != FunctStatus.Abort)
            {
                Funct_results.SetStopTime();
                Funct_results.status = FunctStatus.Fail;
                Funct_status = FunctStatus.Fail;
                Funct_results.StatusText = "Fail";
            }

            // This Will Cause The Interpreter to run any finalization calls..

            // If we failed and have a failure function set... let's run it.
            if ((interpreter.HasFailFunction) && (Funct_results.status == FunctStatus.Fail)) // If a 
            {
                List<FunctResult> failResults = interpreter.RunFailFunction();
                foreach (FunctResult tr in failResults)
                {


                    Funct_results.AddResult(tr);
                    tempFunctResults.Add(tr);


                }

            }

            

            interpreter.Status = ScriptStatus.Finished;

            
            if (this.StatusFinish != null)
            {
                var ew = new StatusString(Funct_results.status.ToString());
                this.StatusFinish(this, ew);
            }
        }
       
        /// <summary>
        /// Causes The Funct Script To Stop Executing
        /// </summary>
        /// <returns>Returns The Funct Result List</returns>
        public FunctResults StopFunct()
        {
            Funct_results.status = FunctStatus.NotRun;
            return Funct_results;
        }




        /// <summary>
        /// The final Funct status
        /// </summary>
        public FunctStatus FunctStatus
        {
            get
            {
                return this.Funct_status;
            }
        }

        /// <summary>
        /// Gets or returns the script name
        /// </summary>
        public string ScriptName
        {
            get
            {
                return this.scriptName;
            }
            set
            {
                this.scriptName = value;
            }
        }

        /// <summary>
        /// Gets the status output
        /// </summary>
        public FunctResults StatusOutputTable
        {
            get
            {
                return this.Funct_results;
            }
        }

        /// <summary>
        /// Returns the Funct results from the Funct script.
        /// </summary>
        public FunctResults FunctResultsList
        {
            get
            {
                return this.Funct_results;
            }
        }



        public VariableCollection variableCollection
        {
            get
            {
                return varCollection;
            }
        }




        /// <summary>
        /// This function compiles the current script
        /// loaded into the Funct script interpreter
        /// </summary>
        /// <returns>True = Success, False = Failure</returns>
        public bool Compile(ref List<CompilerError> compilerErrors)
        {
            return interpreter.Compile(ref compilerErrors);
        }

        public bool ResetElapsedTime()
        {

            elapsedFunctTime = 0;
            return true;
        }

        public double ElapsedTime
        {
            get
            {
                return elapsedFunctTime;
            }
        }



    }

}
