using System.Collections.Generic;

namespace IridiumJS.Parser.Ast
{
    public class CompiledProgram : Statement, IVariableScope, IFunctionScope
    {
        public ICollection<Statement> Body;

        public List<Comment> Comments;
        public List<ParserException> Errors;
        public bool Strict;
        public List<Token> Tokens;

        public CompiledProgram()
        {
            VariableDeclarations = new List<VariableDeclaration>();
        }

        public IList<FunctionDeclaration> FunctionDeclarations { get; set; }

        public IList<VariableDeclaration> VariableDeclarations { get; set; }
    }
}