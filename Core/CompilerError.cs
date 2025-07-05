namespace FunctEngine
{
    public class CompilerError
    {
        public string errorDescription;
        public ScriptLine scriptLine;

        public CompilerError(string error, ScriptLine line)
        {
            errorDescription = error;
            scriptLine = line;
        }
    }
}
