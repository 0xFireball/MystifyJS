namespace Mystifier.DarkMagic.Obfuscators
{
    public abstract class BaseObfuscator
    {
        #region Public Properties

        public string InputSource { get; set; }

        #endregion Public Properties

        #region Public Methods

        public void LoadCode(string code) => InputSource = code;

        public abstract string ObfuscateCode();

        #endregion Public Methods
    }
}