using System.Runtime.Remoting;
using FunctEngine.Enums;
using FunctEngine.Exceptions;
namespace FunctEngine
{
    public class Funct
    {
        protected string FunctName;

        protected string FunctDescription = "Script Step";
        protected FunctStatus runStatus;
        protected VariableCollection variableCollection;
        protected object OtherParameter;
        protected FunctResult FunctResult;
        protected string usage = string.Empty;

        /// <summary>
        /// This intializes the Funct object
        /// </summary>
        /// <param name="FunctName">The name of the Funct</param>
        /// <param name="variableCollection">A copy of the variable space that a given Funct will use</param>
        /// <param name="otherparameter">an object for additional parameters that need to be passed to a function</param>
        public Funct(string name, string description, VariableCollection variableCollection, object otherparameter)
        {
          

            FunctName = this.ToString();
            runStatus = new FunctStatus();
            runStatus = FunctStatus.NotRun;
            if (otherparameter != null)
            {
                OtherParameter = otherparameter;
            }
            // Get the Variable Collection
            this.variableCollection = variableCollection;

            // Let's See if the Funct group name has been set...

            FunctResult = new FunctResult();
            FunctResult.FunctName = this.Name;
            FunctResult.FunctDescription = this.Description;

        }


        public Funct(VariableCollection variableCollection, object otherparameter)
        {
            //
            // TODO: Add constructor logic here
            //

            FunctName = this.ToString();
            runStatus = new FunctStatus();
            runStatus = FunctStatus.NotRun;
            if (otherparameter != null)
            {
                OtherParameter = otherparameter;
            }
            // Get the Variable Space
            this.variableCollection = variableCollection;

            
            FunctResult = new FunctResult();
            FunctResult.FunctName = this.Name;
            FunctResult.FunctDescription = this.Description;


        }

        /// <summary>
        ///The Name Given To The Funct, which Will Be Recorded in the Funct Report. </summary>


        public string Name
        {
            get
            {
                return FunctName;
            }
            set
            {
                FunctName = value;
            }
        }

        /// <summary>
        ///How to use the function. </summary>


        public string Usage
        {
            get
            {
                return usage;
            }
            set
            {
                usage = value;
            }
        }


        /// <summary>
        ///A Short Description of The Funct </summary>

        public string Description
        {
            get
            {
                return this.FunctDescription;
            }
            set
            {
                FunctDescription = value;
            }

        }

        /// <summary>
        /// Returns the runStatus of a Funct
        /// </summary>
        public FunctStatus RunStatus
        {
            get
            {
                return this.runStatus;
            }
            set
            {
                runStatus = value;
            }

        }


        /// <summary>
        /// Executes the Funct and returns the Funct result
        /// </summary>
        /// <returns>The Funct result of executing the Funct</returns>
        public virtual FunctResult Execute()
        {
            FunctResult tr = new FunctResult();
            this.RunStatus = FunctStatus.Pass;
            tr.Status = FunctStatus.Pass;
            tr.FunctName = "Base";
            return tr;
        }

        /// <summary>
        /// Utility Function: This is a internal function that can be used to easily fail
        /// a Funct
        /// </summary>
        /// <param name="message">The fail message to record for the failing Funct</param>
        protected void FailFunct(string message)
        {

            FunctResult.Status = FunctStatus.Fail;
            this.RunStatus = FunctStatus.Fail;
            FunctResult.ErrorMessage = message;


        }

        /// <summary>
        /// Utility Function: This retreives an Integer Variable
        /// </summary>
        /// <param name="VariableName">The name of the variable</param>
        /// <param name="ArgumentName">A brief description of the variable</param>
        /// <returns>The variable contents</returns>
        protected int ReadIntegerVariable(string VariableName, string ArgumentName)
        {
            int retValue = 0;
            FunctVariable myVar;
            try
            {
                myVar = variableCollection.getVariable(VariableName);
                retValue = int.Parse(myVar.Variable.ToString());
                return retValue;
            }
            catch (System.Collections.Generic.KeyNotFoundException)
            {
                throw new VariableReadException(this.Name + ": " + VariableName + " (" + ArgumentName + ") Could Not Be Found.");
            }
            catch (System.FormatException)
            {
                throw new VariableReadException(this.Name + ": " + VariableName + " (" + ArgumentName + ") Could Not Be Cast as an Integer.");
            }
        }


        /// <summary>
        /// Utility Function: This retreives a Global Integer Variable
        /// </summary>
        /// <param name="VariableName">The name of the variable</param>
        /// <param name="ArgumentName">A brief description of the variable</param>
        /// <returns>The variable contents</returns>
        protected int ReadGlobalIntegerVariable(string VariableName, string ArgumentName)
        {
            int retValue = 0;
            FunctVariable myVar;
            try
            {
                myVar = variableCollection.getGlobalVariable(VariableName);
                retValue = int.Parse(myVar.Variable.ToString());
                return retValue;
            }
            catch (System.Collections.Generic.KeyNotFoundException)
            {
                throw new VariableReadException(this.Name + ": " + VariableName + " (" + ArgumentName + ") Could Not Be Found.");
            }
            catch (System.FormatException)
            {
                throw new VariableReadException(this.Name + ": " + VariableName + " (" + ArgumentName + ") Could Not Be Cast as an Integer.");
            }
        }


        /// <summary>
        /// Utility Function: This retreives a Float Variable
        /// </summary>
        /// <param name="VariableName">The name of the variable</param>
        /// <param name="ArgumentName">A brief description of the variable</param>
        /// <returns>The variable contents</returns>
        protected double ReadFloatVariable(string VariableName, string ArgumentName)
        {
            double retValue = 0;
            FunctVariable myVar;
            try
            {
                myVar = variableCollection.getVariable(VariableName);
                retValue = double.Parse(myVar.Variable.ToString());
                return retValue;
            }
            catch (System.Collections.Generic.KeyNotFoundException)
            {
                throw new VariableReadException(this.Name + ": " + VariableName + " (" + ArgumentName + ") Could Not Be Found.");
            }
            catch (System.FormatException)
            {
                throw new VariableReadException(this.Name + ": " + VariableName + " (" + ArgumentName + ") Could Not Be Cast as a Float.");
            }
        }

        /// <summary>
        /// Utility Function: This retreives a Global Float Variable
        /// </summary>
        /// <param name="VariableName">The name of the variable</param>
        /// <param name="ArgumentName">A brief description of the variable</param>
        /// <returns>The variable contents</returns>
        protected double ReadGlobalFloatVariable(string VariableName, string ArgumentName)
        {
            double retValue = 0;
            FunctVariable myVar;
            try
            {
                myVar = variableCollection.getGlobalVariable(VariableName);
                retValue = double.Parse(myVar.Variable.ToString());
                return retValue;
            }
            catch (System.Collections.Generic.KeyNotFoundException)
            {
                throw new VariableReadException(this.Name + ": " + VariableName + " (" + ArgumentName + ") Could Not Be Found.");
            }
            catch (System.FormatException)
            {
                throw new VariableReadException(this.Name + ": " + VariableName + " (" + ArgumentName + ") Could Not Be Cast as a Float.");
            }
        }

        /// <summary>
        /// Utility Function: This retreives a String Variable
        /// </summary>
        /// <param name="VariableName">The name of the variable</param>
        /// <param name="ArgumentName">A brief description of the variable</param>
        /// <returns>The variable contents</returns>
        protected string ReadStringVariable(string VariableName, string ArgumentName)
        {
            string retValue = "";
            FunctVariable myVar;
            try
            {
                myVar = variableCollection.getVariable(VariableName);
                retValue = myVar.Variable.ToString();
                return retValue;
            }
            catch (System.Collections.Generic.KeyNotFoundException)
            {
                throw new VariableReadException(this.Name + ": " + VariableName + " (" + ArgumentName + ") Could Not Be Found.");
            }
        }

        /// <summary>
        /// Utility Function: This retreives a Global String Variable
        /// </summary>
        /// <param name="VariableName">The name of the variable</param>
        /// <param name="ArgumentName">A brief description of the variable</param>
        /// <returns>The variable contents</returns>
        protected string ReadGlobalStringVariable(string VariableName, string ArgumentName)
        {
            string retValue = "";
            FunctVariable myVar;
            try
            {
                myVar = variableCollection.getGlobalVariable(VariableName);
                retValue = myVar.Variable.ToString();
                return retValue;
            }
            catch (System.Collections.Generic.KeyNotFoundException)
            {
                throw new VariableReadException(this.Name + ": " + VariableName + " (" + ArgumentName + ") Could Not Be Found.");
            }
        }

        /// <summary>
        /// Utility Function: This retreives a Boolean Variable
        /// </summary>
        /// <param name="VariableName">The name of the variable</param>
        /// <param name="ArgumentName">A brief description of the variable</param>
        /// <returns>The variable contents</returns>
        protected bool ReadBooleanVariable(string VariableName, string ArgumentName)
        {
            bool retValue = false;
            FunctVariable myVar;
            try
            {
                myVar = variableCollection.getVariable(VariableName);
                retValue = bool.Parse(myVar.Variable.ToString());
                return retValue;
            }
            catch (System.Collections.Generic.KeyNotFoundException)
            {
                throw new VariableReadException(this.Name + ": " + VariableName + " (" + ArgumentName + ") Could Not Be Found.");
            }
            catch (System.FormatException)
            {
                throw new VariableReadException(this.Name + ": " + VariableName + " (" + ArgumentName + ") Could Not Be Cast as a Boolean.");
            }
        }

        /// <summary>
        /// Utility Function: This retreives a Global Boolean Variable
        /// </summary>
        /// <param name="VariableName">The name of the variable</param>
        /// <param name="ArgumentName">A brief description of the variable</param>
        /// <returns>The variable contents</returns>
        protected bool ReadGlobalBooleanVariable(string VariableName, string ArgumentName)
        {
            bool retValue = false;
            FunctVariable myVar;
            try
            {
                myVar = variableCollection.getGlobalVariable(VariableName);
                retValue = bool.Parse(myVar.Variable.ToString());
                return retValue;
            }
            catch (System.Collections.Generic.KeyNotFoundException)
            {
                throw new VariableReadException(this.Name + ": " + VariableName + " (" + ArgumentName + ") Could Not Be Found.");
            }
            catch (System.FormatException)
            {
                throw new VariableReadException(this.Name + ": " + VariableName + " (" + ArgumentName + ") Could Not Be Cast as a Boolean.");
            }
        }

        /// <summary>
        /// Utility Function: This retreives an Object Variable
        /// </summary>
        /// <param name="VariableName">The name of the variable</param>
        /// <param name="ArgumentName">A brief description of the variable</param>
        /// <returns>The variable contents</returns>
        protected object GetObjectVariable(string VariableName, string ArgumentName)
        {
            object retValue;
            FunctVariable myVar;
            try
            {
                myVar = variableCollection.getVariable(VariableName);
                retValue = myVar.Variable;
                return retValue;
            }
            catch (System.Collections.Generic.KeyNotFoundException)
            {
                throw new VariableReadException(this.Name + ": " + VariableName + " (" + ArgumentName + ") Could Not Be Found.");
            }
        }

        /// <summary>
        /// Utility Function: This retreives a Global Object Variable
        /// </summary>
        /// <param name="VariableName">The name of the variable</param>
        /// <param name="ArgumentName">A brief description of the variable</param>
        /// <returns>The variable contents</returns>
        protected object GetGlobalObjectVariable(string VariableName, string ArgumentName)
        {
            object retValue;
            FunctVariable myVar;
            try
            {
                myVar = variableCollection.getGlobalVariable(VariableName);
                retValue = myVar.Variable;
                return retValue;
            }
            catch (System.Collections.Generic.KeyNotFoundException)
            {
                throw new VariableReadException(this.Name + ": " + VariableName + " (" + ArgumentName + ") Could Not Be Found.");
            }
        }


        /// <summary>
        /// Utility Function: This retreives an Array Variable
        /// </summary>
        /// <param name="VariableName">The name of the variable</param>
        /// <param name="ArgumentName">A brief description of the variable</param>
        /// <returns>The variable contents</returns>
        protected System.Array GetArrayVariable(string VariableName, string ArgumentName)
        {

            System.Array retVar;
            FunctVariable myVar;
            try
            {
                myVar = variableCollection.getVariable(VariableName);
                retVar = (System.Array)myVar.Variable;
                return retVar;
            }
            catch (System.Collections.Generic.KeyNotFoundException)
            {
                throw new VariableReadException(this.Name + ": " + VariableName + " (" + ArgumentName + ") Could Not Be Found.");
            }
        }

        /// <summary>
        /// Utility Function: This retreives a Global Array Variable
        /// </summary>
        /// <param name="VariableName">The name of the variable</param>
        /// <param name="ArgumentName">A brief description of the variable</param>
        /// <returns>The variable contents</returns>
        protected System.Array GetGlobalArrayVariable(string VariableName, string ArgumentName)
        {

            System.Array retVar;
            FunctVariable myVar;
            try
            {
                myVar = variableCollection.getGlobalVariable(VariableName);
                retVar = (System.Array)myVar.Variable;
                return retVar;
            }
            catch (System.Collections.Generic.KeyNotFoundException)
            {
                throw new VariableReadException(this.Name + ": " + VariableName + " (" + ArgumentName + ") Could Not Be Found.");
            }
        }

        public int ArgumentCount
        {
            get
            {
                return this.ReadIntegerVariable("ToTalArguments", "Argument Count");
            }
        }

        public static explicit operator Funct(ObjectHandle v)
        {
            throw new NotImplementedException();
        }
    }
}
