namespace Jint.Parser.Ast
{
    public class ConditionalExpression : Expression
    {
        public Expression Alternate;
        public Expression Consequent;
        public Expression Test;
    }
}