using FunctEngine.Enums;
namespace FunctEngine
{
    [Serializable]
    public class ScriptLine
    {
        private string originalText;
        private string text;
        private int lineNumber;
        private string functName;
        private bool breakpoint;
        private Enums.ScriptLineType scriptLineType;
        private Dictionary<string, FunctVariable> arguments;

        public ScriptLine()
        {
            arguments = new Dictionary<string, FunctVariable>();

        }

        public Dictionary<string, FunctVariable> Arguments
        {
            get { return arguments; }
            set { arguments = value; }
        }
        private List<string> outVariableNames;

        public List<string> OutVariableNames
        {
            get { return outVariableNames; }
            set { outVariableNames = value; }
        }

        public ScriptLineType ScriptLineType
        {
            get { return scriptLineType; }
            set { scriptLineType = value; }
        }

        public string Text
        {
            get
            {
                return this.text;
            }
            set
            {
                this.text = value;
            }
        }
        public int LineNumber
        {
            get
            {
                return this.lineNumber;
            }
            set
            {
                this.lineNumber = value;
            }

        }
        public string FunctName
        {
            get
            {
                return functName;
            }
            set
            {
                functName = value;
            }
        }
        public string OriginalText
        {
            get
            {
                return originalText;
            }
            set
            {
                originalText = value;
            }
        }

        private string functFunctionName;

        public string FunctFunctionName
        {
            get { return functFunctionName; }
            set { functFunctionName = value; }
        }

        public bool BreakPoint
        {
            get { return breakpoint; }
            set { breakpoint = value; }
        }

    }
}
