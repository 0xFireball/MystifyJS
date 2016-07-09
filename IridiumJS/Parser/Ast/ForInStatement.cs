namespace Jint.Parser.Ast
{
    public class ForInStatement : Statement
    {
        public Statement Body;
        public bool Each;
        public SyntaxNode Left;
        public Expression Right;
    }
}