using System.Linq;
using System.Net;
using System.Reflection;

namespace Mystifier.JSVM
{
    internal class ExaJSInit
    {
        public static Assembly[] GetZetaJSAssemblies()
        {
            return new[]
            {
                typeof(object).Assembly, //mscorlib.dll
                typeof(WebClient).Assembly, //System.dll
                typeof(Enumerable).Assembly //System.Core.dll
            };
        }
    }
}