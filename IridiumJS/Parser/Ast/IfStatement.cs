namespace Jint.Parser.Ast
{
    public class IfStatement : Statement
    {
        public Statement Alternate;
        public Statement Consequent;
        public Expression Test;
    }
}