namespace Jint.Runtime.Debugger
{
    public class BreakPoint
    {
        public BreakPoint(int line, int character)
        {
            Line = line;
            Char = character;
        }

        public BreakPoint(int line, int character, string condition)
            : this(line, character)
        {
            Condition = condition;
        }

        public int Line { get; set; }
        public int Char { get; set; }
        public string Condition { get; set; }
    }
}