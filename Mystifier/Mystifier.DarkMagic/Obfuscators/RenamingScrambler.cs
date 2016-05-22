using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Mystifier.DarkMagic.Obfuscators
{
    public class RenamingScrambler : BaseObfuscator
    {
        private static readonly string[] ReservedIgnoreNames =
            @"sin,cos,order,min,max,join,round,pow,abs,PI,floor,random,index,http,
    __defineGetter__,__defineSetter__,indexOf,isPrototypeOf,length,clone,toString,split,clear,erase
    RECT,SIZE,Vect,VectInt,vectint,vect,int,double,canvasElement,text1,text2,text3,textSizeTester,target,Number
    number,TimeStep,images,solid,white,default,cursive,fantasy,".
                Split(new[] {Environment.NewLine, " ", "\t", ","}, StringSplitOptions.RemoveEmptyEntries);

        private readonly string[] _extraNames = @"a,b,c".Split(new[] {Environment.NewLine, " ", "\t", ","},
            StringSplitOptions.RemoveEmptyEntries);

        private readonly Dictionary<string, string> _names = new Dictionary<string, string>();


        public override string ObfuscateCode()
        {
            var obfuscatedCode = InputSource;
            var freeVarNameRenameIndex = 0;
            foreach (var s in _extraNames)
            {
                freeVarNameRenameIndex++;
                var freeName = "" + (char) ('A' + freeVarNameRenameIndex%25) +
                               (char) ('A' + freeVarNameRenameIndex/25%25);
                _names.Add(s, freeName);
            }
            var pattern1 = new Regex("(var |function |\\.prototype\\.)([a-zA-Z0-9_]+)");
            var startOfObfuscationPoint = 0;
            while (startOfObfuscationPoint < obfuscatedCode.Length)
            {
                var rgxMatch = pattern1.Match(obfuscatedCode, startOfObfuscationPoint);
                if (!rgxMatch.Success)
                    break;

                var key = obfuscatedCode.Substring(rgxMatch.Groups[2].Index, rgxMatch.Groups[2].Length);
                if (!ReservedIgnoreNames.Contains(key))
                {
                    freeVarNameRenameIndex++;
                    var freeName = "" + (char) ('A' + freeVarNameRenameIndex%25) +
                                   (char) ('A' + freeVarNameRenameIndex/25%25);
                    if (!_names.ContainsKey(key))
                        _names.Add(key, freeName);
                }
                startOfObfuscationPoint = rgxMatch.Groups[0].Index + rgxMatch.Groups[0].Length;
            }

            var pattern2 = new Regex(@"/\*.*\*/", RegexOptions.Multiline);
            var pattern3 = new Regex("([^:\\\\])//.*\r\n");
            var pattern4 = new Regex("([a-zA-Z0-9_]+)");
            var pattern5 = new Regex("(\r\n)*[ \t]+");
            var pattern6 = new Regex("(\r\n)+");
            obfuscatedCode = pattern2.Replace(obfuscatedCode, Scramble_l2);
            obfuscatedCode = pattern3.Replace(obfuscatedCode, ReEvaluate_3);
            obfuscatedCode = pattern4.Replace(obfuscatedCode, ScrambleL4);
            obfuscatedCode = pattern5.Replace(obfuscatedCode, Scramble_5);
            obfuscatedCode = pattern6.Replace(obfuscatedCode, Scramble_l6);
            return obfuscatedCode;
        }

        private string ScrambleL4(Match match)
        {
            return _names.ContainsKey(match.Groups[1].Value) ? _names[match.Groups[1].Value] : match.Groups[0].Value;
        }

        private string Scramble_5(Match match)
        {
            return string.IsNullOrEmpty(match.Groups[1].Value) ? " " : Environment.NewLine;
        }

        private string Scramble_l6(Match match)
        {
            return Environment.NewLine;
        }

        private string Scramble_l2(Match match)
        {
            return " ";
        }

        private string ReEvaluate_3(Match match)
        {
            return match.Groups[1].Value + Environment.NewLine;
        }
    }
}