using System.Text.RegularExpressions;

namespace MystifierLightEditor.SyntaxHighlighting
{
    public class RegexHighlightingDefinition
    {
        public Regex LinePattern { get; set; } = new Regex(".*\\n");
        public Regex Numbers { get; set; } = new Regex("\\b(\\d*[.]?\\d+)\\b");
        public Regex Keywords { get; set; }
        public Regex Builtins { get; set; }
        public Regex Comments { get; set; }
    }
}