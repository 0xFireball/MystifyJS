namespace IridiumJS.Parser.Ast
{
    public class LabelledStatement : Statement
    {
        public Statement Body;
        public Identifier Label;
    }
}