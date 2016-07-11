﻿using IridiumJS.Native.Object;
using IridiumJS.Runtime;
using IridiumJS.Runtime.Environments;

namespace IridiumJS.Native.Function
{
    public abstract class FunctionInstance : ObjectInstance, ICallable
    {
        private readonly IridiumJSEngine _engine;

        protected FunctionInstance(IridiumJSEngine engine, string[] parameters, LexicalEnvironment scope, bool strict)
            : base(engine)
        {
            _engine = engine;
            FormalParameters = parameters;
            Scope = scope;
            Strict = strict;
        }

        public LexicalEnvironment Scope { get; private set; }

        public string[] FormalParameters { get; private set; }
        public bool Strict { get; }

        public override string Class
        {
            get { return "Function"; }
        }

        /// <summary>
        ///     Executed when a function object is used as a function
        /// </summary>
        /// <param name="thisObject"></param>
        /// <param name="arguments"></param>
        /// <returns></returns>
        public abstract JsValue Call(JsValue thisObject, JsValue[] arguments);

        public virtual bool HasInstance(JsValue v)
        {
            var vObj = v.TryCast<ObjectInstance>();
            if (vObj == null)
            {
                return false;
            }

            var po = Get("prototype");
            if (!po.IsObject())
            {
                throw new JavaScriptException(_engine.TypeError,
                    string.Format("Function has non-object prototype '{0}' in instanceof check",
                        TypeConverter.ToString(po)));
            }

            var o = po.AsObject();

            if (o == null)
            {
                throw new JavaScriptException(_engine.TypeError);
            }

            while (true)
            {
                vObj = vObj.Prototype;

                if (vObj == null)
                {
                    return false;
                }
                if (vObj == o)
                {
                    return true;
                }
            }
        }

        /// <summary>
        ///     http://www.ecma-international.org/ecma-262/5.1/#sec-15.3.5.4
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public override JsValue Get(string propertyName)
        {
            var v = base.Get(propertyName);

            var f = v.As<FunctionInstance>();
            if (propertyName == "caller" && f != null && f.Strict)
            {
                throw new JavaScriptException(_engine.TypeError);
            }

            return v;
        }
    }
}