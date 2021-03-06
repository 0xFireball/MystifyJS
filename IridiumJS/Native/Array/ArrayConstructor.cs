﻿using System.Collections;
using IridiumJS.Native.Function;
using IridiumJS.Native.Object;
using IridiumJS.Runtime;
using IridiumJS.Runtime.Interop;

namespace IridiumJS.Native.Array
{
    public sealed class ArrayConstructor : FunctionInstance, IConstructor
    {
        private ArrayConstructor(IridiumJSEngine engine) : base(engine, null, null, false)
        {
        }

        public ArrayPrototype PrototypeObject { get; private set; }

        public override JsValue Call(JsValue thisObject, JsValue[] arguments)
        {
            return Construct(arguments);
        }

        public ObjectInstance Construct(JsValue[] arguments)
        {
            var instance = new ArrayInstance(Engine);
            instance.Prototype = PrototypeObject;
            instance.Extensible = true;

            if (arguments.Length == 1 && arguments.At(0).IsNumber())
            {
                var length = TypeConverter.ToUint32(arguments.At(0));
                if (!TypeConverter.ToNumber(arguments[0]).Equals(length))
                {
                    throw new JavaScriptException(Engine.RangeError, "Invalid array length");
                }

                instance.FastAddProperty("length", length, true, false, false);
            }
            else if (arguments.Length == 1 && arguments.At(0).IsObject() && arguments.At(0).As<ObjectWrapper>() != null)
            {
                var enumerable = arguments.At(0).As<ObjectWrapper>().Target as IEnumerable;

                if (enumerable != null)
                {
                    var jsArray = Engine.Array.Construct(Arguments.Empty);
                    foreach (var item in enumerable)
                    {
                        var jsItem = JsValue.FromObject(Engine, item);
                        Engine.Array.PrototypeObject.Push(jsArray, Arguments.From(jsItem));
                    }

                    return jsArray;
                }
            }
            else
            {
                instance.FastAddProperty("length", 0, true, false, false);
                PrototypeObject.Push(instance, arguments);
            }

            return instance;
        }

        public static ArrayConstructor CreateArrayConstructor(IridiumJSEngine engine)
        {
            var obj = new ArrayConstructor(engine);
            obj.Extensible = true;

            // The value of the [[Prototype]] internal property of the Array constructor is the Function prototype object 
            obj.Prototype = engine.Function.PrototypeObject;
            obj.PrototypeObject = ArrayPrototype.CreatePrototypeObject(engine, obj);

            obj.FastAddProperty("length", 1, false, false, false);

            // The initial value of Array.prototype is the Array prototype object
            obj.FastAddProperty("prototype", obj.PrototypeObject, false, false, false);

            return obj;
        }

        public void Configure()
        {
            FastAddProperty("isArray", new ClrFunctionInstance(Engine, IsArray, 1), true, false, true);
        }

        private JsValue IsArray(JsValue thisObj, JsValue[] arguments)
        {
            if (arguments.Length == 0)
            {
                return false;
            }

            var o = arguments.At(0);

            return o.IsObject() && o.AsObject().Class == "Array";
        }
    }
}