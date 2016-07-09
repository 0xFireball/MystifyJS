namespace Jint.Parser.Ast
{
    public class LabelledStatement : Statement
    {
        public Statement Body;
        public Identifier Label;
    }
}