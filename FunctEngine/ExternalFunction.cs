using System.Reflection;

namespace FunctEngine;

public class ExternalFunction
{
    public string Name { get; set; }
    public string Description { get; set; }
    public int ParameterCount { get; set; } = -1; // -1 = parámetros variables
    public MethodInfo Method { get; set; }
    public Func<object[], object> BuiltInAction { get; set; }
}