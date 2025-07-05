using System.Reflection;

namespace FunctEngine
{
    public class FunctList
    {
        SortedDictionary<string, Type> functionDictionary;

        public SortedDictionary<string, Type> FunctionDictionary
        {
            get { return functionDictionary; }

        }

        public FunctList()
        {
            BuildMappingList();
        }

        public void BuildMappingList()
        {
            string appPath = System.AppDomain.CurrentDomain.BaseDirectory;
            functionDictionary = new SortedDictionary<string, Type>();

            // bool errorOccurred = false;

            List<string> modules = new List<string>();
            modules.Add("CoreFunctions.dll");
            foreach (string mod in modules)
            {

                string  libraryFile = appPath + mod;
                string functionName = "";
                try
                {
                    Assembly functionAssembly = Assembly.LoadFile(libraryFile);
                    Type[] functions = functionAssembly.GetTypes();
                    foreach (Type function in functions)
                    {
                        functionName = function.Name;
                        if (function.IsSubclassOf(Type.GetType("FunctEngine.Funct")))
                        {
                            functionDictionary.Add(functionName, function);
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    string errMsg = "Error Loading Library Function [" + functionName + " ] In Library [" + libraryFile + "]: " + ex.Message;
                    System.Diagnostics.Trace.WriteLine(errMsg);
                    // errorOccurred = true;
                }
            }
            // End of Function Loading Loop
            
        }
        /// <summary>
		/// Returns a Funct from the script mapping list based on the Funct name.
		/// </summary>
		/// <param name="Functname">The name of the Funct to get</param>
		/// <param name="varSpace">The current variable space in use by the Funct script</param>
		/// <param name="csa">The CellSynchronization agent for the Funct executive</param>
		/// <returns>The Funct object requested by name.</returns>
		virtual public Funct GetFunct(string Functname, VariableCollection varSpace)
        {
            Type tempType = functionDictionary[Functname];
            Funct myFunct = (Funct)Activator.CreateInstance(tempType, new object[] { varSpace, null });
            myFunct.Name = Functname;
            return myFunct;
        }

        /// <summary>
        /// Returns true/false if the Funct exists
        /// </summary>
        /// <param name="Functname">The name of the Funct</param>
        /// <returns>Does the script exist?</returns>
        virtual public bool FunctExists(string Functname)
        {
            return functionDictionary.ContainsKey(Functname);
        }
    }
}
