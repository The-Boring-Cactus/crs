using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FunctEngine
{
    public class ArrayFunctions
    {
        public object CreateArray(object[] args)
        {
            return new List<object>(args);
        }

        public object GetLength(object[] args)
        {
            if (args[0] is List<object> list) return list.Count;
            return 0;
        }

        public object GetElement(object[] args)
        {
            if (args[0] is List<object> list && args.Length > 1)
            {
                int index = Convert.ToInt32(args[1]);
                return index >= 0 && index < list.Count ? list[index] : null;
            }
            return null;
        }

        public object SetElement(object[] args)
        {
            if (args[0] is List<object> list && args.Length > 2)
            {
                int index = Convert.ToInt32(args[1]);
                if (index >= 0 && index < list.Count)
                {
                    list[index] = args[2];
                    return args[2];
                }
            }
            return null;
        }

        public object Push(object[] args)
        {
            if (args[0] is List<object> list && args.Length > 1)
            {
                list.Add(args[1]);
                return list.Count;
            }
            return 0;
        }

        public object Pop(object[] args)
        {
            if (args[0] is List<object> list && list.Count > 0)
            {
                var last = list[list.Count - 1];
                list.RemoveAt(list.Count - 1);
                return last;
            }
            return null;
        }

        public object Slice(object[] args)
        {
            if (args[0] is List<object> list && args.Length >= 3)
            {
                int start = Convert.ToInt32(args[1]);
                int count = Convert.ToInt32(args[2]);
                start = Math.Max(0, Math.Min(start, list.Count));
                count = Math.Max(0, Math.Min(count, list.Count - start));
                return list.GetRange(start, count);
            }
            return new List<object>();
        }

        public object Join(object[] args)
        {
            if (args[0] is List<object> list)
            {
                string separator = args.Length > 1 ? args[1]?.ToString() ?? "," : ",";
                return string.Join(separator, list.Select(x => x?.ToString() ?? ""));
            }
            return "";
        }

        public object Sort(object[] args)
        {
            if (args[0] is List<object> list)
            {
                var sorted = list.OrderBy(x => {
                    if (double.TryParse(x?.ToString(), out double d)) return d;
                    return 0;
                }).ToList();
                return sorted;
            }
            return new List<object>();
        }

        public object Reverse(object[] args)
        {
            if (args[0] is List<object> list)
            {
                var reversed = new List<object>(list);
                reversed.Reverse();
                return reversed;
            }
            return new List<object>();
        }
    }
}
