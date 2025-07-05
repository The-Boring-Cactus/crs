using FunctEngine.Enums;

namespace FunctEngine
{
    [Serializable]
    public class FunctVariable
    {
        private string name;
        private VariableType variabletype;
        private object myobject;


        /// <summary>
        /// The container for Script Variables
        /// </summary>
        public FunctVariable()
        {
            //
            // TODO: Add constructor logic here
            //
            name = "";
            variabletype = VariableType.String;
            myobject = null;
        }

        /// <summary>
        /// The container for Script Variables
        /// </summary>
        /// <param name="Name">The Name of The Script Variable</param>
        /// <param name="varType">The Type of the Script Variable</param>
        /// <param name="Variable">The object to store in the Script Variable</param>
        public FunctVariable(string Name, VariableType varType, object Variable)
        {
            name = Name;
            variabletype = varType;
            myobject = Variable;
        }


        /// <summary>
        /// The name of the script variable
        /// </summary>
        public string Name
        {
            get
            { return this.name; }
            set
            { this.name = value; }
        }
        /// <summary>
        /// The variable type of the script variable
        /// </summary>
        public VariableType variableType
        {
            get
            {
                return this.variabletype;
            }
            set
            {
                variabletype = value;
            }
        }
        /// <summary>
        /// The actual contents of the script variable
        /// </summary>
        public object Variable
        {
            get
            {
                return this.myobject;
            }
            set
            {
                this.myobject = value;
            }
        }
        /// <summary>
        /// An overrided "ToString" function which allows for returning the
        /// script variable as a string.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.name;
        }

    }
}