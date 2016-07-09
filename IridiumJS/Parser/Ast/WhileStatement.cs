namespace Jint.Parser.Ast
{
    public class WhileStatement : Statement
    {
        public Statement Body;
        public Expression Test;
    }
}