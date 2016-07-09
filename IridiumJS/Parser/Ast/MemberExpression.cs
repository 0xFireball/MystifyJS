namespace Jint.Parser.Ast
{
    public class MemberExpression : Expression
    {
        // true if an indexer is used and the property to be evaluated
        public bool Computed;
        public Expression Object;
        public Expression Property;
    }
}