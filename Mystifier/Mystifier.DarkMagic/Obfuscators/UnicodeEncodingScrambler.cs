using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Mystifier.DarkMagic.Obfuscators
{
    public class UnicodeEncodingScrambler : BaseObfuscator
    {
        public override string ObfuscateCode()
        {
            var obfuscatedCode = new StringBuilder(InputSource);
            //Scrambler level 1
            obfuscatedCode = UnicodeEscapeString(obfuscatedCode);
            var decodingJS = new StringBuilder(@"
var e = ""$$$""
var r = /\\u([\d\w]{4})/gi;
e = e.replace(r, function (match, grp) {
    return String.fromCharCode(parseInt(grp, 16)); } );
e = unescape(e);
eval(e);
");
            obfuscatedCode = decodingJS.Replace("$$$", obfuscatedCode.ToString());
            return obfuscatedCode.ToString();
        }

        private static StringBuilder UnicodeEscapeString(StringBuilder value)
        {
            var sb = new StringBuilder();
            foreach (var c in value.ToString())
            {
                var encodedValue = "\\u" + ((int)c).ToString("x4");
                sb.Append(encodedValue);
            }
            return sb;
        }

        private static string UnicodeUnescapeString(string value)
        {
            return Regex.Replace(
                value,
                @"\\u(?<Value>[a-zA-Z0-9]{4})",
                m => ((char)int.Parse(m.Groups["Value"].Value, NumberStyles.HexNumber)).ToString());
        }
    }
}