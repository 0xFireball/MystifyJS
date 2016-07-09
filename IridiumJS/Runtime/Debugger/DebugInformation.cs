using System;
using System.Collections.Generic;
using IridiumJS.Native;
using IridiumJS.Parser.Ast;

namespace IridiumJS.Runtime.Debugger
{
    public class DebugInformation : EventArgs
    {
        public Stack<string> CallStack { get; set; }
        public Statement CurrentStatement { get; set; }
        public Dictionary<string, JsValue> Locals { get; set; }
        public Dictionary<string, JsValue> Globals { get; set; }
    }
}