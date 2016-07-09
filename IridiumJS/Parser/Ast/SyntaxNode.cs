using System.Diagnostics;

namespace IridiumJS.Parser.Ast
{
    public class SyntaxNode
    {
        public Location Location;
        public int[] Range;
        public SyntaxNodes Type;

        [DebuggerStepThrough]
        public T As<T>() where T : SyntaxNode
        {
            return (T) this;
        }
    }
}