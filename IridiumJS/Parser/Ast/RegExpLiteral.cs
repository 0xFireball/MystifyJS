namespace Jint.Parser.Ast
{
    public class RegExpLiteral : Expression, IPropertyKeyExpression
    {
        public string Flags;
        public string Raw;
        public object Value;

        public string GetKey()
        {
            return Value.ToString();
        }
    }
}