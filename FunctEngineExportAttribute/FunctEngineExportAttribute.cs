namespace FunctEngineExportAttribute
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class FunctEngineExportAttribute : Attribute
    {
        public string Name { get; set; }
        public string Description { get; set; }

        public FunctEngineExportAttribute(string name = null, string description = "")
        {
            Name = name;
            Description = description;
        }
    }
}
