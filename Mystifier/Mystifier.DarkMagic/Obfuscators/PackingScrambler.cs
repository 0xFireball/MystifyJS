using System;
using System.Collections;
using System.Collections.Specialized;
using System.Text;
using System.Text.RegularExpressions;

namespace Mystifier.DarkMagic.Obfuscators
{
    public class PackingScrambler : BaseObfuscator
    {
        /// <summary>
        ///     Packs a javascript file into a smaller area, removing unnecessary characters from the output.
        /// </summary>
        public class EcmaScriptPacker
        {
            /// <summary>
            ///     The encoding level to use. See http://dean.edwards.name/packer/usage/ for more info.
            /// </summary>
            public enum PackerEncoding
            {
                None = 0,
                Numeric = 10,
                Mid = 36,
                Normal = 62,
                HighAscii = 95
            }

            //lookups seemed like the easiest way to do this since
            // I don't know of an equivalent to .toString(36)
            private static readonly string Lookup36 = "0123456789abcdefghijklmnopqrstuvwxyz";

            private static readonly string Lookup62 = Lookup36 + "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

            private static readonly string Lookup95 =
                "¡¢£¤¥¦§¨©ª«¬­®¯°±²³´µ¶·¸¹º»¼½¾¿ÀÁÂÃÄÅÆÇÈÉÊËÌÍÎÏÐÑÒÓÔÕÖ×ØÙÚÛÜÝÞßàáâãäåæçèéêëìíîïðñòóôõö÷øùúûüýþÿ";

            private WordList _encodingLookup;

            private readonly string _ignore = "$1";

            public EcmaScriptPacker()
            {
                Encoding = PackerEncoding.Normal;
                FastDecode = true;
                SpecialChars = false;
            }

            /// <summary>
            ///     Constructor
            /// </summary>
            /// <param name="encoding">The encoding level for this instance</param>
            /// <param name="fastDecode">Adds a subroutine to the output to speed up decoding</param>
            /// <param name="specialChars">Replaces special characters</param>
            public EcmaScriptPacker(EcmaScriptPacker.PackerEncoding encoding, bool fastDecode, bool specialChars)
            {
                Encoding = encoding;
                FastDecode = fastDecode;
                SpecialChars = specialChars;
            }

            /// <summary>
            ///     The encoding level for this instance
            /// </summary>
            public PackerEncoding Encoding { get; set; } = PackerEncoding.Normal;

            /// <summary>
            ///     Adds a subroutine to the output to speed up decoding
            /// </summary>
            public bool FastDecode { get; set; } = true;

            /// <summary>
            ///     Replaces special characters
            /// </summary>
            public bool SpecialChars { get; set; }

            /// <summary>
            ///     Packer enabled
            /// </summary>
            public bool Enabled { get; set; } = true;

            /// <summary>
            ///     Packs the script
            /// </summary>
            /// <param name="script">the script to pack</param>
            /// <returns>the packed script</returns>
            public string Pack(string script)
            {
                if (Enabled)
                {
                    script += "\n";
                    script = BasicCompression(script);
                    if (SpecialChars)
                        script = EncodeSpecialChars(script);
                    if (Encoding != PackerEncoding.None)
                        script = EncodeKeywords(script);
                }
                return script;
            }

            //zero encoding - just removal of whitespace and comments
            private string BasicCompression(string script)
            {
                var parser = new ParseMaster();
                // make safe
                parser.EscapeChar = '\\';
                // protect strings
                parser.Add("'[^'\\n\\r]*'", _ignore);
                parser.Add("\"[^\"\\n\\r]*\"", _ignore);
                // remove comments
                parser.Add("\\/\\/[^\\n\\r]*[\\n\\r]");
                parser.Add("\\/\\*[^*]*\\*+([^\\/][^*]*\\*+)*\\/");
                // protect regular expressions
                parser.Add("\\s+(\\/[^\\/\\n\\r\\*][^\\/\\n\\r]*\\/g?i?)", "$2");
                parser.Add("[^\\w\\$\\/'\"*)\\?:]\\/[^\\/\\n\\r\\*][^\\/\\n\\r]*\\/g?i?", _ignore);
                // remove: ;;; doSomething();
                if (SpecialChars)
                    parser.Add(";;[^\\n\\r]+[\\n\\r]");
                // remove redundant semi-colons
                parser.Add(";+\\s*([};])", "$2");
                // remove white-space
                parser.Add("(\\b|\\$)\\s+(\\b|\\$)", "$2 $3");
                parser.Add("([+\\-])\\s+([+\\-])", "$2 $3");
                parser.Add("\\s+");
                // done
                return parser.Exec(script);
            }

            private string EncodeSpecialChars(string script)
            {
                var parser = new ParseMaster();
                // replace: $name -> n, $$name -> na
                parser.Add("((\\$+)([a-zA-Z\\$_]+))(\\d*)",
                    EncodeLocalVars);

                // replace: _name -> _0, double-underscore (__name) is ignored
                var regex = new Regex("\\b_[A-Za-z\\d]\\w*");

                // build the word list
                _encodingLookup = Analyze(script, regex, EncodePrivate);

                parser.Add("\\b_[A-Za-z\\d]\\w*", EncodeWithLookup);

                script = parser.Exec(script);
                return script;
            }

            private string EncodeKeywords(string script)
            {
                // escape high-ascii values already in the script (i.e. in strings)
                if (Encoding == PackerEncoding.HighAscii) script = Escape95(script);
                // create the parser
                var parser = new ParseMaster();
                var encode = GetEncoder(Encoding);

                // for high-ascii, don't encode single character low-ascii
                var regex = new Regex(
                    Encoding == PackerEncoding.HighAscii ? "\\w\\w+" : "\\w+"
                    );
                // build the word list
                _encodingLookup = Analyze(script, regex, encode);

                // encode
                parser.Add(Encoding == PackerEncoding.HighAscii ? "\\w\\w+" : "\\w+",
                    EncodeWithLookup);

                // if encoded, wrap the script in a decoding function
                return script == string.Empty ? "" : BootStrap(parser.Exec(script), _encodingLookup);
            }

            private string BootStrap(string packed, WordList keywords)
            {
                // packed: the packed script
                packed = "'" + Escape(packed) + "'";

                // ascii: base for encoding
                var ascii = Math.Min(keywords.Sorted.Count, (int) Encoding);
                if (ascii == 0)
                    ascii = 1;

                // count: number of words contained in the script
                var count = keywords.Sorted.Count;

                // keywords: list of words contained in the script
                foreach (var key in keywords.Protected.Keys)
                {
                    keywords.Sorted[(int) key] = "";
                }
                // convert from a string to an array
                var sbKeywords = new StringBuilder("'");
                foreach (var word in keywords.Sorted)
                    sbKeywords.Append(word + "|");
                sbKeywords.Remove(sbKeywords.Length - 1, 1);
                var keywordsout = sbKeywords + "'.split('|')";

                string encode;
                var inline = "c";

                switch (Encoding)
                {
                    case PackerEncoding.Mid:
                        encode = "function(c){return c.toString(36)}";
                        inline += ".toString(a)";
                        break;

                    case PackerEncoding.Normal:
                        encode = "function(c){return(c<a?\"\":e(parseInt(c/a)))+" +
                                 "((c=c%a)>35?String.fromCharCode(c+29):c.toString(36))}";
                        inline += ".toString(a)";
                        break;

                    case PackerEncoding.HighAscii:
                        encode = "function(c){return(c<a?\"\":e(c/a))+" +
                                 "String.fromCharCode(c%a+161)}";
                        inline += ".toString(a)";
                        break;

                    default:
                        encode = "function(c){return c}";
                        break;
                }

                // decode: code snippet to speed up decoding
                var decode = "";
                if (FastDecode)
                {
                    decode =
                        "if(!''.replace(/^/,String)){while(c--)d[e(c)]=k[c]||e(c);k=[function(e){return d[e]}];e=function(){return'\\\\w+'};c=1;}";
                    if (Encoding == PackerEncoding.HighAscii)
                        decode = decode.Replace("\\\\w", "[\\xa1-\\xff]");
                    else if (Encoding == PackerEncoding.Numeric)
                        decode = decode.Replace("e(c)", inline);
                    if (count == 0)
                        decode = decode.Replace("c=1", "c=0");
                }

                // boot function
                var unpack =
                    "function(p,a,c,k,e,d){while(c--)if(k[c])p=p.replace(new RegExp('\\\\b'+e(c)+'\\\\b','g'),k[c]);return p;}";
                Regex r;
                if (FastDecode)
                {
                    //insert the decoder
                    r = new Regex("\\{");
                    unpack = r.Replace(unpack, "{" + decode + ";", 1);
                }

                if (Encoding == PackerEncoding.HighAscii)
                {
                    // get rid of the word-boundries for regexp matches
                    r = new Regex("'\\\\\\\\b'\\s*\\+|\\+\\s*'\\\\\\\\b'");
                    unpack = r.Replace(unpack, "");
                }
                if (Encoding == PackerEncoding.HighAscii || ascii > (int) PackerEncoding.Normal || FastDecode)
                {
                    // insert the encode function
                    r = new Regex("\\{");
                    unpack = r.Replace(unpack, "{e=" + encode + ";", 1);
                }
                else
                {
                    r = new Regex("e\\(c\\)");
                    unpack = r.Replace(unpack, inline);
                }
                // no need to pack the boot function since i've already done it
                var _params = "" + packed + "," + ascii + "," + count + "," + keywordsout;
                if (FastDecode)
                {
                    //insert placeholders for the decoder
                    _params += ",0,{}";
                }
                // the whole thing
                return "eval(" + unpack + "(" + _params + "))\n";
            }

            private string Escape(string input)
            {
                var r = new Regex("([\\\\'])");
                return r.Replace(input, "\\$1");
            }

            private EncodeMethod GetEncoder(PackerEncoding encoding)
            {
                switch (encoding)
                {
                    case PackerEncoding.Mid:
                        return Encode36;

                    case PackerEncoding.Normal:
                        return Encode62;

                    case PackerEncoding.HighAscii:
                        return Encode95;

                    default:
                        return Encode10;
                }
            }

            private string Encode10(int code)
            {
                return code.ToString();
            }

            private string Encode36(int code)
            {
                var encoded = "";
                var i = 0;
                do
                {
                    var digit = code/(int) Math.Pow(36, i)%36;
                    encoded = Lookup36[digit] + encoded;
                    code -= digit*(int) Math.Pow(36, i++);
                } while (code > 0);
                return encoded;
            }

            private string Encode62(int code)
            {
                var encoded = "";
                var i = 0;
                do
                {
                    var digit = code/(int) Math.Pow(62, i)%62;
                    encoded = Lookup62[digit] + encoded;
                    code -= digit*(int) Math.Pow(62, i++);
                } while (code > 0);
                return encoded;
            }

            private string Encode95(int code)
            {
                var encoded = "";
                var i = 0;
                do
                {
                    var digit = code/(int) Math.Pow(95, i)%95;
                    encoded = Lookup95[digit] + encoded;
                    code -= digit*(int) Math.Pow(95, i++);
                } while (code > 0);
                return encoded;
            }

            private string Escape95(string input)
            {
                var r = new Regex("[\xa1-\xff]");
                return r.Replace(input, Escape95Eval);
            }

            private string Escape95Eval(Match match)
            {
                return "\\x" + ((int) match.Value[0]).ToString("x"); //return hexadecimal value
            }

            private string EncodeLocalVars(Match match, int offset)
            {
                var length = match.Groups[offset + 2].Length;
                var start = length - Math.Max(length - match.Groups[offset + 3].Length, 0);
                return match.Groups[offset + 1].Value.Substring(start, length) +
                       match.Groups[offset + 4].Value;
            }

            private string EncodeWithLookup(Match match, int offset)
            {
                return (string) _encodingLookup.Encoded[match.Groups[offset].Value];
            }

            private string EncodePrivate(int code)
            {
                return "_" + code;
            }

            private WordList Analyze(string input, Regex regex, EncodeMethod encodeMethod)
            {
                // analyse
                // retreive all words in the script
                var all = regex.Matches(input);
                WordList rtrn;
                rtrn.Sorted = new StringCollection(); // list of words sorted by frequency
                rtrn.Protected = new HybridDictionary(); // dictionary of word->encoding
                rtrn.Encoded = new HybridDictionary(); // instances of "protected" words
                if (all.Count > 0)
                {
                    var unsorted = new StringCollection(); // same list, not sorted
                    var Protected = new HybridDictionary(); // "protected" words (dictionary of word->"word")
                    var values = new HybridDictionary(); // dictionary of charCode->encoding (eg. 256->ff)
                    var count = new HybridDictionary(); // word->count
                    int i = all.Count, j = 0;
                    string word;
                    // count the occurrences - used for sorting later
                    do
                    {
                        word = "$" + all[--i].Value;
                        if (count[word] == null)
                        {
                            count[word] = 0;
                            unsorted.Add(word);
                            // make a dictionary of all of the protected words in this script
                            //  these are words that might be mistaken for encoding
                            Protected["$" + (values[j] = encodeMethod(j))] = j++;
                        }
                        // increment the word counter
                        count[word] = (int) count[word] + 1;
                    } while (i > 0);
                    /* prepare to sort the word list, first we must protect
                        words that are also used as codes. we assign them a code
                        equivalent to the word itself.
                       e.g. if "do" falls within our encoding range
                            then we store keywords["do"] = "do";
                       this avoids problems when decoding */
                    i = unsorted.Count;
                    var sortedarr = new string[unsorted.Count];
                    do
                    {
                        word = unsorted[--i];
                        if (Protected[word] != null)
                        {
                            sortedarr[(int) Protected[word]] = word.Substring(1);
                            rtrn.Protected[(int) Protected[word]] = true;
                            count[word] = 0;
                        }
                    } while (i > 0);
                    var unsortedarr = new string[unsorted.Count];
                    unsorted.CopyTo(unsortedarr, 0);
                    // sort the words by frequency
                    Array.Sort(unsortedarr, new CountComparer(count));
                    j = 0;
                    /*because there are "protected" words in the list
                      we must add the sorted words around them */
                    do
                    {
                        if (sortedarr[i] == null)
                            sortedarr[i] = unsortedarr[j++].Substring(1);
                        rtrn.Encoded[sortedarr[i]] = values[i];
                    } while (++i < unsortedarr.Length);
                    rtrn.Sorted.AddRange(sortedarr);
                }
                return rtrn;
            }

            private delegate string EncodeMethod(int code);

            private struct WordList
            {
                public StringCollection Sorted;
                public HybridDictionary Encoded;
                public HybridDictionary Protected;
            }

            private class CountComparer : IComparer
            {
                private readonly HybridDictionary _count;

                public CountComparer(HybridDictionary count)
                {
                    this._count = count;
                }

                #region IComparer Members

                public int Compare(object x, object y)
                {
                    return (int) _count[y] - (int) _count[x];
                }

                #endregion IComparer Members
            }
        }

        /// <summary>
        ///     a multi-pattern parser
        /// </summary>
        public class ParseMaster
        {
            /// <summary>
            ///     Delegate to call when a regular expression is found.
            ///     Use match.Groups[offset + &lt;group number&gt;].Value to get
            ///     the correct subexpression
            /// </summary>
            public delegate string MatchGroupEvaluator(Match match, int offset);

            private readonly StringCollection _escaped = new StringCollection();
            // used to determine nesting levels
            private readonly Regex _groups = new Regex("\\(");

            private readonly Regex _subReplace = new Regex("\\$");

            private readonly Regex _indexed = new Regex("^\\$\\d+$");

            private readonly Regex _escape = new Regex("\\\\.");

            private Regex _quote = new Regex("'");

            private readonly Regex _deleted = new Regex("\\x01[^\\x01]*\\x01");

            private readonly ArrayList _patterns = new ArrayList();

            //decode escaped characters
            private int _unescapeIndex;

            /// <summary>
            ///     Ignore Case?
            /// </summary>
            public bool IgnoreCase { get; set; } = false;

            /// <summary>
            ///     Escape Character to use
            /// </summary>
            public char EscapeChar { get; set; } = '\0';

            private string Delete(Match match, int offset)
            {
                return "\x01" + match.Groups[offset].Value + "\x01";
            }

            /// <summary>
            ///     Add an expression to be deleted
            /// </summary>
            /// <param name="expression">Regular Expression String</param>
            public void Add(string expression)
            {
                Add(expression, string.Empty);
            }

            /// <summary>
            ///     Add an expression to be replaced with the replacement string
            /// </summary>
            /// <param name="expression">Regular Expression String</param>
            /// <param name="replacement">Replacement String. Use $1, $2, etc. for groups</param>
            public void Add(string expression, string replacement)
            {
                if (replacement == string.Empty)
                    add(expression, new MatchGroupEvaluator(Delete));

                add(expression, replacement);
            }

            /// <summary>
            ///     Add an expression to be replaced using a callback function
            /// </summary>
            /// <param name="expression">Regular expression string</param>
            /// <param name="replacement">Callback function</param>
            public void Add(string expression, MatchGroupEvaluator replacement)
            {
                add(expression, replacement);
            }

            /// <summary>
            ///     Executes the parser
            /// </summary>
            /// <param name="input">input string</param>
            /// <returns>parsed string</returns>
            public string Exec(string input)
            {
                return _deleted.Replace(Unescape(GetPatterns().Replace(Escape(input), Replacement)), string.Empty);
                //long way for debugging
                /*input = escape(input);
                Regex patterns = getPatterns();
                input = patterns.Replace(input, new MatchEvaluator(replacement));
                input = DELETED.Replace(input, string.Empty);
                return input;*/
            }

            private void add(string expression, object replacement)
            {
                var pattern = new Pattern();
                pattern.Expression = expression;
                pattern.Replacement = replacement;
                //count the number of sub-expressions
                // - add 1 because each group is itself a sub-expression
                pattern.Length = _groups.Matches(InternalEscape(expression)).Count + 1;

                //does the pattern deal with sup-expressions?
                if (replacement is string && _subReplace.IsMatch((string) replacement))
                {
                    var sreplacement = (string) replacement;
                    // a simple lookup (e.g. $2)
                    if (_indexed.IsMatch(sreplacement))
                    {
                        pattern.Replacement = int.Parse(sreplacement.Substring(1)) - 1;
                    }
                }

                _patterns.Add(pattern);
            }

            /// <summary>
            ///     builds the patterns into a single regular expression
            /// </summary>
            /// <returns></returns>
            private Regex GetPatterns()
            {
                var rtrn = new StringBuilder(string.Empty);
                foreach (var pattern in _patterns)
                {
                    rtrn.Append((Pattern) pattern + "|");
                }
                rtrn.Remove(rtrn.Length - 1, 1);
                return new Regex(rtrn.ToString(), IgnoreCase ? RegexOptions.IgnoreCase : RegexOptions.None);
            }

            /// <summary>
            ///     Global replacement function. Called once for each match found
            /// </summary>
            /// <param name="match">Match found</param>
            private string Replacement(Match match)
            {
                int i = 1, j = 0;
                Pattern pattern;
                //loop through the patterns
                while (!((pattern = (Pattern) _patterns[j++]) == null))
                {
                    //do we have a result?
                    if (match.Groups[i].Value != string.Empty)
                    {
                        var replacement = pattern.Replacement;
                        if (replacement is MatchGroupEvaluator)
                        {
                            return ((MatchGroupEvaluator) replacement)(match, i);
                        }
                        if (replacement is int)
                        {
                            return match.Groups[(int) replacement + i].Value;
                        }
                        //string, send to interpreter
                        return ReplacementString(match, i, (string) replacement, pattern.Length);
                    }
                    i += pattern.Length;
                }
                return match.Value; //should never be hit, but you never know
            }

            /// <summary>
            ///     Replacement function for complicated lookups (e.g. Hello $3 $2)
            /// </summary>
            private string ReplacementString(Match match, int offset, string replacement, int length)
            {
                while (length > 0)
                {
                    replacement = replacement.Replace("$" + length--, match.Groups[offset + length].Value);
                }
                return replacement;
            }

            //encode escaped characters
            private string Escape(string str)
            {
                if (EscapeChar == '\0')
                    return str;
                var escaping = new Regex("\\\\(.)");
                return escaping.Replace(str, EscapeMatch);
            }

            private string EscapeMatch(Match match)
            {
                _escaped.Add(match.Groups[1].Value);
                return "\\";
            }

            private string Unescape(string str)
            {
                if (EscapeChar == '\0')
                    return str;
                var unescaping = new Regex("\\" + EscapeChar);
                return unescaping.Replace(str, UnescapeMatch);
            }

            private string UnescapeMatch(Match match)
            {
                return "\\" + _escaped[_unescapeIndex++];
            }

            private string InternalEscape(string str)
            {
                return _escape.Replace(str, "");
            }

            //subclass for each pattern
            private class Pattern
            {
                public string Expression;
                public int Length;
                public object Replacement;

                public override string ToString()
                {
                    return "(" + Expression + ")";
                }
            }
        }

        public override string ObfuscateCode()
        {
            var obfuscatedCode = InputSource;
            EcmaScriptPacker p = new EcmaScriptPacker(EcmaScriptPacker.PackerEncoding.HighAscii, true, true);
            obfuscatedCode = p.Pack(obfuscatedCode);
            return obfuscatedCode;
        }
    }
}