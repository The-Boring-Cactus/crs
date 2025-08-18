namespace FunctEngine;

public class FunctEngineGlobalContext
{
    public static object GetVariable(string name)
    {
        var interpreter = RuntimeInterpreter.GetCurrentInstance();
        return interpreter?.GetVariable(name);
    }

    public static void SetVariable(string name, object value)
    {
        var interpreter = RuntimeInterpreter.GetCurrentInstance();
        interpreter?.SetVariable(name, value);
    }

    public static bool HasVariable(string name)
    {
        var interpreter = RuntimeInterpreter.GetCurrentInstance();
        return interpreter?.HasVariable(name) ?? false;
    }

    public static void RemoveVariable(string name)
    {
        var interpreter = RuntimeInterpreter.GetCurrentInstance();
        interpreter?.RemoveVariable(name);
    }

    public static Dictionary<string, object> GetAllVariables()
    {
        var interpreter = RuntimeInterpreter.GetCurrentInstance();
        return interpreter?.GetAllVariables() ?? new Dictionary<string, object>();
    }

    public static void Print(object value)
    {
        Console.WriteLine(value?.ToString() ?? "null");
    }

    public static T GetVariable<T>(string name, T defaultValue = default(T))
    {
        var value = GetVariable(name);
        if (value == null) return defaultValue;
            
        try
        {
            if (typeof(T) == typeof(string))
                return (T)(object)value.ToString();
                
            return (T)Convert.ChangeType(value, typeof(T));
        }
        catch
        {
            return defaultValue;
        }
    }
}