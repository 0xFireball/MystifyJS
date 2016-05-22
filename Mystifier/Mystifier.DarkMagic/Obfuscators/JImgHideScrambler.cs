using System;
using System.Text;

namespace Mystifier.DarkMagic.Obfuscators
{
    internal class JImgHideScrambler : BaseObfuscator
    {
        public override string ObfuscateCode()
        {
            var obfuscatedCode = InputSource;
            var srcB64 = Base64Encode(obfuscatedCode);
            return obfuscatedCode;
        }

        private static string Base64Encode(string plainText)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }
    }
}