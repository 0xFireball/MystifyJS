namespace IridiumJS.Parser.Ast
{
    public class ForStatement : Statement
    {
        public Statement Body;
        // can be a Statement (var i) or an Expression (i=0)
        public SyntaxNode Init;
        public Expression Test;
        public Expression Update;
    }
}