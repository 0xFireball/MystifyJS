using Android.Graphics;
using Java.Util.Regex;

namespace MystifierLightEditor.SyntaxHighlighting
{
    public class PatternBasedHighlightingDefinition
    {
        public Pattern LinePattern { get; set; } = Pattern.Compile(".*\\n");
        public Pattern NumbersPattern { get; set; } = Pattern.Compile("\\b(\\d*[.]?\\d+)\\b");
        public Pattern KeywordPattern { get; set; } = Pattern.Compile(@"/\b(export|default|from|var|let)\b/g");
        public Pattern BuiltinsPattern { get; set; } = Pattern.Compile(@"/\b(window|document)\b/g");
        public Pattern CommentsPattern { get; set; } = Pattern.Compile(@"/\/\*[\s\S]*?\*\/|(\/\/)[\s\S]*?$/gm");

        public Color ErrorColor { get; set; } = new Color(0xff0000);
        public Color NumberColor { get; set; } = new Color(0xae81ff);
        public Color KeywordColor { get; set; } = new Color(0xf92672);
        public Color BuiltinsColor { get; set; } = new Color(0x66d9ef);
        public Color CommentsColor { get; set; } = new Color(0x75715e);
    }
}