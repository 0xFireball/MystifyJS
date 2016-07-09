namespace Jint.Parser.Ast
{
    public class WithStatement : Statement
    {
        public Statement Body;
        public Expression Object;
    }
}