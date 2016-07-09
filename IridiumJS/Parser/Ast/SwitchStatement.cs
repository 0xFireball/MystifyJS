using System.Collections.Generic;

namespace IridiumJS.Parser.Ast
{
    public class SwitchStatement : Statement
    {
        public IEnumerable<SwitchCase> Cases;
        public Expression Discriminant;
    }
}