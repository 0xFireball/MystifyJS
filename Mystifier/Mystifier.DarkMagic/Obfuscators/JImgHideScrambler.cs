using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mystifier.DarkMagic.Obfuscators
{
    class JImgHideScrambler : BaseObfuscator
    {
        public override string ObfuscateCode()
        {
            var obfuscatedCode = InputSource;
            string srcB64 = Base64Encode(obfuscatedCode);
            return obfuscatedCode;
        }

        private static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }
    }
}
