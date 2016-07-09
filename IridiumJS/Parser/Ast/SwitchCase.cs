using System.Collections.Generic;

namespace Jint.Parser.Ast
{
    public class SwitchCase : SyntaxNode
    {
        public IEnumerable<Statement> Consequent;
        public Expression Test;
    }
}