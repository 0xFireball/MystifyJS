using IridiumJS.Native;
using IridiumJS.Parser.Ast;

namespace IridiumJS.Runtime.CallStack
{
    public class CallStackElement
    {
        private readonly string _shortDescription;

        public CallStackElement(CallExpression callExpression, JsValue function, string shortDescription)
        {
            _shortDescription = shortDescription;
            CallExpression = callExpression;
            Function = function;
        }

        public CallExpression CallExpression { get; private set; }

        public JsValue Function { get; private set; }

        public override string ToString()
        {
            return _shortDescription;
        }
    }
}