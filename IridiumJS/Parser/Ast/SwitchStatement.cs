using System.Collections.Generic;

namespace Jint.Parser.Ast
{
    public class SwitchStatement : Statement
    {
        public IEnumerable<SwitchCase> Cases;
        public Expression Discriminant;
    }
}