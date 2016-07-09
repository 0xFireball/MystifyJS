using System.Collections.Generic;

namespace IridiumJS.Parser.Ast
{
    public class SwitchCase : SyntaxNode
    {
        public IEnumerable<Statement> Consequent;
        public Expression Test;
    }
}