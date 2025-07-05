using FunctEngine.Enums;

namespace FunctEngine
{
    [Serializable]
    public class FunctResult
    {
        public string FunctDescription = "";
        public string FunctName = "";
        public string ErrorMessage = "";
        public FunctStatus Status = new FunctStatus();
        public double executionTime = 0;
        public double elapsedTime = 0;
        public FunctResult()
        {
            //
            // TODO: Add constructor logic here
            //
        }
    }
}
