﻿using System;
using IridiumJS.Native;
using IridiumJS.Runtime.Interop;

namespace IridiumJS.Runtime.Descriptors.Specialized
{
    public sealed class ClrAccessDescriptor : PropertyDescriptor
    {
        public ClrAccessDescriptor(Engine engine, Func<JsValue, JsValue> get)
            : this(engine, get, null)
        {
        }

        public ClrAccessDescriptor(Engine engine, Func<JsValue, JsValue> get, Action<JsValue, JsValue> set)
            : base(
                new GetterFunctionInstance(engine, get),
                set == null ? Native.Undefined.Instance : new SetterFunctionInstance(engine, set)
                )
        {
        }
    }
}