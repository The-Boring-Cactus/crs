using Jint;
using Jint.Native;
namespace FunctEngine
{
    public class Evaluator
    {
        public int EvalToInteger(string statement)
        {
            string s = EvalToString(statement);
            return int.Parse(s.ToString());
        }

        public double EvalToDouble(string statement)
        {
            string s = EvalToString(statement);
            return double.Parse(s);
        }

        public string EvalToString(string statement)
        {
            object o = EvalToObject(statement);
            return o.ToString();
        }

        public object EvalToObject(string statement)
        {


            return _evaluator.Call(statement);
        }

        public Evaluator()
        {

            _evaluator = new Engine().Execute(_jscriptSource).GetValue("eval");

        }

        private JsValue _evaluator = null;
        private readonly string _jscriptSource =

            @"function Eval(expr) 
                  { 
                     return eval(expr); 
                  }";
    }
}
