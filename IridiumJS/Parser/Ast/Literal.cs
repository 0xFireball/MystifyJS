using IridiumJS.Native;

namespace IridiumJS.Parser.Ast
{
    public class Literal : Expression, IPropertyKeyExpression
    {
        public bool Cached;
        public JsValue CachedValue;
        public string Raw;
        public object Value;

        public string GetKey()
        {
            return Value.ToString();
        }
    }
}