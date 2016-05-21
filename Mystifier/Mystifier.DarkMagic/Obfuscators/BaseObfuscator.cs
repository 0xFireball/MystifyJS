namespace Mystifier.DarkMagic.Obfuscators
{
    public abstract class BaseObfuscator
    {
        protected BaseObfuscator(string inputCode)
        {
            InputSource = inputCode;
        }

        public string InputSource { get; set; }

        public abstract string ObfuscateCode();
    }
}