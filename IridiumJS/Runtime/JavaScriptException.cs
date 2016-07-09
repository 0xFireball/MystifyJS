using System;
using IridiumJS.Native;
using IridiumJS.Native.Error;
using IridiumJS.Parser;

namespace IridiumJS.Runtime
{
    public class JavaScriptException : Exception
    {
        public JavaScriptException(ErrorConstructor errorConstructor) : base("")
        {
            Error = errorConstructor.Construct(Arguments.Empty);
        }

        public JavaScriptException(ErrorConstructor errorConstructor, string message)
            : base(message)
        {
            Error = errorConstructor.Construct(new JsValue[] {message});
        }

        public JavaScriptException(JsValue error)
            : base(GetErrorMessage(error))
        {
            Error = error;
        }

        public JsValue Error { get; }

        public Location Location { get; set; }

        public int LineNumber
        {
            get { return null == Location ? 0 : Location.Start.Line; }
        }

        public int Column
        {
            get { return null == Location ? 0 : Location.Start.Column; }
        }

        private static string GetErrorMessage(JsValue error)
        {
            if (error.IsObject())
            {
                var oi = error.AsObject();
                var message = oi.Get("message").AsString();
                return message;
            }
            return string.Empty;
        }

        public override string ToString()
        {
            return Error.ToString();
        }
    }
}