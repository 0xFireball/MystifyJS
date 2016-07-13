using Android.Graphics;
using Java.Util.Regex;

namespace MystifierLightEditor.SyntaxHighlighting
{
    public class PatternBasedHighlightingDefinition
    {
        public Pattern LinePattern { get; set; } = Pattern.Compile(".*\\n");
        public Pattern NumbersPattern { get; set; } = Pattern.Compile("\\b(\\d*[.]?\\d+)\\b");
        public Pattern StringPattern { get; set; } = Pattern.Compile(@"/("")(.*?)\1/g");
        public Pattern KeywordPattern { get; set; } = Pattern.Compile(@"/\b(export|default|from|var|let)\b/g");
        public Pattern BuiltinsPattern { get; set; } = Pattern.Compile(@"/\b(window|document)\b/g");
        public Pattern CommentsPattern { get; set; } = Pattern.Compile(@"/\/\*[\s\S]*?\*\/|(\/\/)[\s\S]*?$/gm");

        public Color ErrorColor { get; set; } = Color.ParseColor("#FF0000");
        public Color NumberColor { get; set; } = Color.ParseColor("#AE81FF");
        public Color StringColor { get; set; } = Color.ParseColor("#E6DB74");
        public Color KeywordColor { get; set; } = Color.ParseColor("#F92672");
        public Color BuiltinsColor { get; set; } = Color.ParseColor("#66D9EF");
        public Color CommentsColor { get; set; } = Color.ParseColor("#75715E");
    }
}