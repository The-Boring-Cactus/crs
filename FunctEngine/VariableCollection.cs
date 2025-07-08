using System.Text;
using FunctEngine.Enums;

namespace FunctEngine
{
    public class VariableCollection
    {
        private Dictionary<string, FunctVariable> scriptVariables;

        private GlobalVariables globalVariables;
        public VariableCollection(GlobalVariables globalVariables)
        {
            scriptVariables = new Dictionary<string, FunctVariable>();
            FunctVariable FunctStatus = new FunctVariable();
            FunctStatus.Name = "FunctStatus";
            FunctStatus.variableType = VariableType.String;
            FunctStatus.Variable = new StringBuilder();
            setVariable(FunctStatus);
            this.globalVariables = globalVariables;
        }
        public void TryFocus(string name)
        {
            globalVariables.Tryfocus(name);
        }
        public bool VariableExists(string varName)
        {
            return scriptVariables.ContainsKey(varName);
        }
        public bool GlobalVariableExists(string varName)
        {
            return globalVariables.VariableExists(varName);
        }
        public FunctVariable getVariable(string name)
        { 
            return scriptVariables[name]; 
        }
        public FunctVariable getGlobalVariable(string name)
        {
            return globalVariables.getVariable(name);
        }
        public void setVariable(FunctVariable variable)
        {
            if (this.scriptVariables.ContainsKey(variable.Name))
            {
                FunctVariable scriptVariable = this.scriptVariables[variable.Name];
                scriptVariable.Variable = variable.Variable;
                scriptVariable.variableType = variable.variableType;
            }
            else {
                this.scriptVariables.Add(variable.Name, variable);
            }
        }
        public void setGlobalVariable(FunctVariable variable)
        {
            globalVariables.SetVariable(variable);
        }
        public void SetArgument(int argumentIndex, FunctVariable variable)
        {
            this.setVariable(new FunctVariable("Argument" + argumentIndex.ToString(), variable.variableType, variable.Variable));
        }
        /// <summary>
        /// This returns an arraylist of all the Funct variables
        /// </summary>
        public Dictionary<string, FunctVariable> Variables
        {
            get
            {
                return scriptVariables;
            }

        }
        public event StatusUpdateHandler StatusUpdate;

      
      
        public void UpdaFunctatusText(string updaFunctring)
        {

            FunctVariable variable1 = this.getVariable("FunctStatus");
            StringBuilder variable2 = (StringBuilder)variable1.Variable;
            variable2.Append("\r\n" + updaFunctring);
            variable1.Variable = variable2;
            this.setVariable(variable1);
            if (this.StatusUpdate != null)
            {
                StatusString statusString = new StatusString(updaFunctring);
                this.StatusUpdate(this, statusString);
            }
        }

    }
}
