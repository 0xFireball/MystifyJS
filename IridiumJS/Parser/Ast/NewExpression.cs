using System.Collections.Generic;

namespace IridiumJS.Parser.Ast
{
    public class NewExpression : Expression
    {
        public IEnumerable<Expression> Arguments;
        public Expression Callee;
    }
}