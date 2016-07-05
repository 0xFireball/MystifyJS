using System.Linq;
using System.Net;
using System.Reflection;

namespace Mystifier.DarkMagic.JSVM
{
    public class ExaJSInit
    {
        public static Assembly[] GetExaJSAssemblies()
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