﻿using System;
using IridiumJS.Native;
using IridiumJS.Native.Function;

namespace IridiumJS.Runtime.Interop
{
    /// <summary>
    ///     Represents a FunctionInstance wrapping a Clr setter.
    /// </summary>
    public sealed class SetterFunctionInstance : FunctionInstance
    {
        private readonly Action<JsValue, JsValue> _setter;

        public SetterFunctionInstance(IridiumJSEngine engine, Action<JsValue, JsValue> setter)
            : base(engine, null, null, false)
        {
            _setter = setter;
        }

        public override JsValue Call(JsValue thisObject, JsValue[] arguments)
        {
            _setter(thisObject, arguments[0]);

            return Null.Instance;
        }
    }
}