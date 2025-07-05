namespace FunctEngine
{
    public class FunctionInstance
    {
        private string functionName;
        private int startLocation;
        private int endLocation;
        private ScriptLine scriptLine;

        public ScriptLine FunctionScriptLine
        {
            get
            {
                return scriptLine;
            }
            set
            {
                scriptLine = value;
            }

        }


        public string FunctionName
        {
            get
            {
                return functionName;
            }
            set
            {
                functionName = value;
            }


        }

        public int StartLocation
        {
            get
            {
                return startLocation;
            }
            set
            {
                startLocation = value;
            }
        }

        public int EndLocation
        {
            get
            {
                return endLocation;
            }
            set
            {
                endLocation = value;
            }
        }
    }
}
