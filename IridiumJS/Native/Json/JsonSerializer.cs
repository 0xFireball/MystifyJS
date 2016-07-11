﻿using System;
using System.Collections.Generic;
using System.Linq;
using IridiumJS.Native.Array;
using IridiumJS.Native.Global;
using IridiumJS.Native.Object;
using IridiumJS.Runtime;
using IridiumJS.Runtime.Descriptors;

namespace IridiumJS.Native.Json
{
    public class JsonSerializer
    {
        private readonly IridiumJSEngine _engine;
        private string _indent;
        private string _gap;
        private List<string> _propertyList;
        private JsValue _replacerFunction = Undefined.Instance;

        private Stack<object> _stack;

        public JsonSerializer(IridiumJSEngine engine)
        {
            _engine = engine;
        }

        public JsValue Serialize(JsValue value, JsValue replacer, JsValue space)
        {
            _stack = new Stack<object>();

            // for JSON.stringify(), any function passed as the first argument will return undefined
            // if the replacer is not defined. The function is not called either.
            if (value.Is<ICallable>() && replacer == Undefined.Instance)
            {
                return Undefined.Instance;
            }

            if (replacer.IsObject())
            {
                if (replacer.Is<ICallable>())
                {
                    _replacerFunction = replacer;
                }
                else
                {
                    var replacerObj = replacer.AsObject();
                    if (replacerObj.Class == "Array")
                    {
                        _propertyList = new List<string>();
                    }

                    foreach (var property in replacerObj.GetOwnProperties().Select(x => x.Value))
                    {
                        var v = _engine.GetValue(property);
                        string item = null;
                        if (v.IsString())
                        {
                            item = v.AsString();
                        }
                        else if (v.IsNumber())
                        {
                            item = TypeConverter.ToString(v);
                        }
                        else if (v.IsObject())
                        {
                            var propertyObj = v.AsObject();
                            if (propertyObj.Class == "String" || propertyObj.Class == "Number")
                            {
                                item = TypeConverter.ToString(v);
                            }
                        }

                        if (item != null && !_propertyList.Contains(item))
                        {
                            _propertyList.Add(item);
                        }
                    }
                }
            }

            if (space.IsObject())
            {
                var spaceObj = space.AsObject();
                if (spaceObj.Class == "Number")
                {
                    space = TypeConverter.ToNumber(spaceObj);
                }
                else if (spaceObj.Class == "String")
                {
                    space = TypeConverter.ToString(spaceObj);
                }
            }

            // defining the gap
            if (space.IsNumber())
            {
                if (space.AsNumber() > 0)
                {
                    _gap = new string(' ', (int) System.Math.Min(10, space.AsNumber()));
                }
                else
                {
                    _gap = string.Empty;
                }
            }
            else if (space.IsString())
            {
                var stringSpace = space.AsString();
                _gap = stringSpace.Length <= 10 ? stringSpace : stringSpace.Substring(0, 10);
            }
            else
            {
                _gap = string.Empty;
            }

            var wrapper = _engine.Object.Construct(Arguments.Empty);
            wrapper.DefineOwnProperty("", new PropertyDescriptor(value, true, true, true), false);

            return Str("", wrapper);
        }

        private JsValue Str(string key, ObjectInstance holder)
        {
            var value = holder.Get(key);
            if (value.IsObject())
            {
                var toJson = value.AsObject().Get("toJSON");
                if (toJson.IsObject())
                {
                    var callableToJson = toJson.AsObject() as ICallable;
                    if (callableToJson != null)
                    {
                        value = callableToJson.Call(value, Arguments.From(key));
                    }
                }
            }

            if (_replacerFunction != Undefined.Instance)
            {
                var replacerFunctionCallable = (ICallable) _replacerFunction.AsObject();
                value = replacerFunctionCallable.Call(holder, Arguments.From(key, value));
            }


            if (value.IsObject())
            {
                var valueObj = value.AsObject();
                switch (valueObj.Class)
                {
                    case "Number":
                        value = TypeConverter.ToNumber(value);
                        break;
                    case "String":
                        value = TypeConverter.ToString(value);
                        break;
                    case "Boolean":
                        value = TypeConverter.ToPrimitive(value);
                        break;
                    case "Array":
                        value = SerializeArray(value.As<ArrayInstance>());
                        return value;
                    case "Object":
                        value = SerializeObject(value.AsObject());
                        return value;
                }
            }

            if (value == Null.Instance)
            {
                return "null";
            }

            if (value.IsBoolean() && value.AsBoolean())
            {
                return "true";
            }

            if (value.IsBoolean() && !value.AsBoolean())
            {
                return "false";
            }

            if (value.IsString())
            {
                return Quote(value.AsString());
            }

            if (value.IsNumber())
            {
                if (GlobalObject.IsFinite(Undefined.Instance, Arguments.From(value)).AsBoolean())
                {
                    return TypeConverter.ToString(value);
                }

                return "null";
            }

            var isCallable = value.IsObject() && value.AsObject() is ICallable;

            if (value.IsObject() && isCallable == false)
            {
                if (value.AsObject().Class == "Array")
                {
                    return SerializeArray(value.As<ArrayInstance>());
                }

                return SerializeObject(value.AsObject());
            }

            return JsValue.Undefined;
        }

        private string Quote(string value)
        {
            var product = "\"";

            foreach (var c in value)
            {
                switch (c)
                {
                    case '\"':
                        product += "\\\"";
                        break;
                    case '\\':
                        product += "\\\\";
                        break;
                    case '\b':
                        product += "\\b";
                        break;
                    case '\f':
                        product += "\\f";
                        break;
                    case '\n':
                        product += "\\n";
                        break;
                    case '\r':
                        product += "\\r";
                        break;
                    case '\t':
                        product += "\\t";
                        break;
                    default:
                        if (c < 0x20)
                        {
                            product += "\\u";
                            product += ((int) c).ToString("x4");
                        }
                        else
                            product += c;
                        break;
                }
            }

            product += "\"";
            return product;
        }

        private string SerializeArray(ArrayInstance value)
        {
            EnsureNonCyclicity(value);
            _stack.Push(value);
            var stepback = _indent;
            _indent = _indent + _gap;
            var partial = new List<string>();
            var len = TypeConverter.ToUint32(value.Get("length"));
            for (var i = 0; i < len; i++)
            {
                var strP = Str(TypeConverter.ToString(i), value);
                if (strP == JsValue.Undefined)
                    strP = "null";
                partial.Add(strP.AsString());
            }
            if (partial.Count == 0)
            {
                return "[]";
            }

            string final;
            if (_gap == "")
            {
                var separator = ",";
                var properties = string.Join(separator, partial.ToArray());
                final = "[" + properties + "]";
            }
            else
            {
                var separator = ",\n" + _indent;
                var properties = string.Join(separator, partial.ToArray());
                final = "[\n" + _indent + properties + "\n" + stepback + "]";
            }

            _stack.Pop();
            _indent = stepback;
            return final;
        }

        private void EnsureNonCyclicity(object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            if (_stack.Contains(value))
            {
                throw new JavaScriptException(_engine.TypeError, "Cyclic reference detected.");
            }
        }

        private string SerializeObject(ObjectInstance value)
        {
            string final;

            EnsureNonCyclicity(value);
            _stack.Push(value);
            var stepback = _indent;
            _indent += _gap;

            var k = _propertyList ?? value.GetOwnProperties()
                .Where(x => x.Value.Enumerable.HasValue && x.Value.Enumerable.Value)
                .Select(x => x.Key)
                .ToList();

            var partial = new List<string>();
            foreach (var p in k)
            {
                var strP = Str(p, value);
                if (strP != JsValue.Undefined)
                {
                    var member = Quote(p) + ":";
                    if (_gap != "")
                    {
                        member += " ";
                    }
                    member += strP.AsString(); // TODO:This could be undefined
                    partial.Add(member);
                }
            }
            if (partial.Count == 0)
            {
                final = "{}";
            }
            else
            {
                if (_gap == "")
                {
                    var separator = ",";
                    var properties = string.Join(separator, partial.ToArray());
                    final = "{" + properties + "}";
                }
                else
                {
                    var separator = ",\n" + _indent;
                    var properties = string.Join(separator, partial.ToArray());
                    final = "{\n" + _indent + properties + "\n" + stepback + "}";
                }
            }
            _stack.Pop();
            _indent = stepback;
            return final;
        }
    }
}