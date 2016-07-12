using Android.Graphics;
using Java.Util.Regex;

namespace MystifierLightEditor.SyntaxHighlighting
{
    public class RegexHighlightingDefinition
    {
        public Pattern LinePattern { get; set; } = Pattern.Compile(".*\\n");
        public Pattern NumbersPattern { get; set; } = Pattern.Compile("\\b(\\d*[.]?\\d+)\\b");
        public Pattern KeywordsPattern { get; set; }
        public Pattern BuiltinsPattern { get; set; }
        public Pattern CommentsPattern { get; set; }

        public Color ErrorColor { get; set; }
        public Color NumberColor { get; set; }
        public Color KeywordColor { get; set; }
        public Color BuiltinColor { get; set; }
        public Color CommentColor { get; set; }
    }
}