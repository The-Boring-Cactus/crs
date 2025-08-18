using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FunctEngine
{
    public class StringFunctions
    {
        public object GetLength(object[] args)
        {
            return args[0]?.ToString()?.Length ?? 0;
        }

        public object Substring(object[] args)
        {
            string str = args[0]?.ToString() ?? "";
            int start = Convert.ToInt32(args[1]);
            if (args.Length > 2)
            {
                int length = Convert.ToInt32(args[2]);
                start = Math.Max(0, Math.Min(start, str.Length));
                length = Math.Max(0, Math.Min(length, str.Length - start));
                return str.Substring(start, length);
            }
            start = Math.Max(0, Math.Min(start, str.Length));
            return str.Substring(start);
        }

        public object IndexOf(object[] args)
        {
            string str = args[0]?.ToString() ?? "";
            string search = args[1]?.ToString() ?? "";
            return str.IndexOf(search);
        }

        public object ToUpper(object[] args)
        {
            return args[0]?.ToString()?.ToUpper() ?? "";
        }

        public object ToLower(object[] args)
        {
            return args[0]?.ToString()?.ToLower() ?? "";
        }

        public object Trim(object[] args)
        {
            return args[0]?.ToString()?.Trim() ?? "";
        }

        public object Replace(object[] args)
        {
            string str = args[0]?.ToString() ?? "";
            string oldValue = args[1]?.ToString() ?? "";
            string newValue = args[2]?.ToString() ?? "";
            return str.Replace(oldValue, newValue);
        }

        public object Split(object[] args)
        {
            string str = args[0]?.ToString() ?? "";
            string separator = args.Length > 1 ? args[1]?.ToString() ?? " " : " ";
            return new List<object>(str.Split(new string[] { separator }, StringSplitOptions.RemoveEmptyEntries));
        }

        public object StartsWith(object[] args)
        {
            string str = args[0]?.ToString() ?? "";
            string prefix = args[1]?.ToString() ?? "";
            return str.StartsWith(prefix);
        }

        public object EndsWith(object[] args)
        {
            string str = args[0]?.ToString() ?? "";
            string suffix = args[1]?.ToString() ?? "";
            return str.EndsWith(suffix);
        }

        public object Contains(object[] args)
        {
            string str = args[0]?.ToString() ?? "";
            string search = args[1]?.ToString() ?? "";
            return str.Contains(search);
        }

        public object PadLeft(object[] args)
        {
            string str = args[0]?.ToString() ?? "";
            int totalWidth = Convert.ToInt32(args[1]);
            char paddingChar = args.Length > 2 ? args[2]?.ToString()?[0] ?? ' ' : ' ';
            return str.PadLeft(totalWidth, paddingChar);
        }

        public object PadRight(object[] args)
        {
            string str = args[0]?.ToString() ?? "";
            int totalWidth = Convert.ToInt32(args[1]);
            char paddingChar = args.Length > 2 ? args[2]?.ToString()?[0] ?? ' ' : ' ';
            return str.PadRight(totalWidth, paddingChar);
        }
    }
}
