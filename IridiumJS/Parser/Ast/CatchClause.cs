namespace Jint.Parser.Ast
{
    public class CatchClause : Statement
    {
        public BlockStatement Body;
        public Identifier Param;
    }
}