using System.Collections.Generic;

namespace Jint.Parser.Ast
{
    public class NewExpression : Expression
    {
        public IEnumerable<Expression> Arguments;
        public Expression Callee;
    }
}