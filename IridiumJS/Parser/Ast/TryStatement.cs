using System.Collections.Generic;

namespace IridiumJS.Parser.Ast
{
    public class TryStatement : Statement
    {
        public Statement Block;
        public Statement Finalizer;
        public IEnumerable<Statement> GuardedHandlers;
        public IEnumerable<CatchClause> Handlers;
    }
}