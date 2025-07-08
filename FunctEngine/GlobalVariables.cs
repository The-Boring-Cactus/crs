namespace FunctEngine
{
    
    public class GlobalVariables
    {
        public event StatusUpdateHandler StatusUpdate;
        private Dictionary<string, FunctVariable> scriptVariables;
        public GlobalVariables() {
            scriptVariables = new Dictionary<string, FunctVariable>();
        }

        public void Tryfocus(string name)
        {
            if(StatusUpdate != null)
            {
                var e = new StatusString(name);
                StatusUpdate(this, e);
            }
        }
        public bool VariableExists(string varName)
        {
            return scriptVariables.ContainsKey(varName);
        }
        public FunctVariable getVariable(string name)
        {
            return scriptVariables[name];
        }
        public void SetVariable(FunctVariable variable)
        {
           
            if (this.scriptVariables.ContainsKey(variable.Name))
            {
                FunctVariable scriptVariable = this.scriptVariables[variable.Name];
                scriptVariable.Variable = variable.Variable;
                scriptVariable.variableType = variable.variableType;
            }
            else
            {
                this.scriptVariables.Add(variable.Name, variable);
            }
            
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

    }
}
