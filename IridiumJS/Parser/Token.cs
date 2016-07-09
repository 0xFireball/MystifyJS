namespace IridiumJS.Parser
{
    public enum Tokens
    {
        BooleanLiteral = 1,
        EOF = 2,
        Identifier = 3,
        Keyword = 4,
        NullLiteral = 5,
        NumericLiteral = 6,
        Punctuator = 7,
        StringLiteral = 8,
        RegularExpression = 9
    }

    public class Token
    {
        public static Token Empty = new Token();
        public int? LineNumber;
        public int LineStart;
        public string Literal;
        public Location Location;
        public bool Octal;
        public int Precedence;
        public int[] Range;

        public Tokens Type;
        public object Value;
    }
}