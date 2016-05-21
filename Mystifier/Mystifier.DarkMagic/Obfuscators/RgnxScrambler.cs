using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Mystifier.DarkMagic.Obfuscators
{
    public class RgnxScrambler : BaseObfuscator
    {
        public RgnxScrambler(string inputCode) : base(inputCode)
        {
        }

        private static readonly string[] IgnoreNames =
            @"sin,cos,order,min,max,join,round,pow,abs,PI,floor,random,index,http,
    __defineGetter__,__defineSetter__,indexOf,isPrototypeOf,length,clone,toString,split,clear,erase
    RECT,SIZE,Vect,VectInt,vectint,vect,int,double,canvasElement,text1,text2,text3,textSizeTester,target,Number
    number,TimeStep,images,solid,white,default,cursive,fantasy,".
          Split(new string[] { Environment.NewLine, " ", "\t", "," }, StringSplitOptions.RemoveEmptyEntries);

        private readonly string[] _extraNames = @"a,b,c".Split(new string[] { Environment.NewLine, " ", "\t", "," }, StringSplitOptions.RemoveEmptyEntries);
        private readonly Dictionary<string, string> _names = new Dictionary<string, string>();

        public override string ObfuscateCode()
        {
            string obfuscatedCode = InputSource;
            int freeIndex = 0;
            foreach (string s in _extraNames)
            {
                freeIndex++;
                string freeName = "" + (char)('A' + (freeIndex % 25)) + (char)('A' + ((freeIndex / 25) % 25));
                _names.Add(s, freeName);
            }
            Regex reg1 = new Regex("(var |function |\\.prototype\\.)([a-zA-Z0-9_]+)");
            int startat = 0;
            while (startat < obfuscatedCode.Length)
            {
                Match match = reg1.Match(obfuscatedCode, startat);
                if (!match.Success)
                    break;

                string key = obfuscatedCode.Substring(match.Groups[2].Index, match.Groups[2].Length);
                if (!IgnoreNames.Contains(key))
                {
                    freeIndex++;
                    string freeName = "" + (char)('A' + (freeIndex % 25)) + (char)('A' + ((freeIndex / 25) % 25));
                    if (!_names.ContainsKey(key))
                        _names.Add(key, freeName);
                }
                startat = match.Groups[0].Index + match.Groups[0].Length;
            }

            Regex reg2 = new Regex(@"/\*.*\*/", RegexOptions.Multiline);
            Regex reg3 = new Regex("([^:\\\\])//.*\r\n");
            Regex reg4 = new Regex("([a-zA-Z0-9_]+)");
            Regex reg5 = new Regex("(\r\n)*[ \t]+");
            Regex reg6 = new Regex("(\r\n)+");
            obfuscatedCode = reg2.Replace(obfuscatedCode, Eval2);
            obfuscatedCode = reg3.Replace(obfuscatedCode, Eval3);
            obfuscatedCode = reg4.Replace(obfuscatedCode, Eval4);
            obfuscatedCode = reg5.Replace(obfuscatedCode, Eval5);
            obfuscatedCode = reg6.Replace(obfuscatedCode, Eval6);
            return obfuscatedCode;
        }

        private string Eval4(Match match)
        {
            return _names.ContainsKey(match.Groups[1].Value) ? _names[match.Groups[1].Value] : match.Groups[0].Value;
        }

        private string Eval5(Match match)
        {
            return string.IsNullOrEmpty(match.Groups[1].Value) ? " " : Environment.NewLine;
        }

        private string Eval6(Match match)
        {
            return Environment.NewLine;
        }

        private string Eval2(Match match)
        {
            return " ";
        }

        private string Eval3(Match match)
        {
            return match.Groups[1].Value + Environment.NewLine;
        }
    }
}