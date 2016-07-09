using System.Collections.Generic;
using Jint.Native;

namespace Jint.Parser.Ast
{
    public class CallExpression : Expression
    {
        public IList<Expression> Arguments;

        public bool Cached;
        public JsValue[] CachedArguments;
        public Expression Callee;
        public bool CanBeCached = true;
    }
}