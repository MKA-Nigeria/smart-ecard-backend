using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Helper;
public static class ClassGenerator
{
    public static dynamic CreateClass(string[] keys)
    {
        dynamic expando = new ExpandoObject();
        foreach (string key in keys)
        {
            IDictionary<string, object> expandoDict = (IDictionary<string, object>)expando;
            expandoDict[key] = null;
        }
        return expando;
    }
}
