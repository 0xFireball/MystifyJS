﻿using Jint.Native.Object;
using Jint.Runtime;

namespace Jint.Native.Boolean
{
    public class BooleanInstance : ObjectInstance, IPrimitiveInstance
    {
        public BooleanInstance(Engine engine)
            : base(engine)
        {
        }

        public override string Class
        {
            get { return "Boolean"; }
        }

        public JsValue PrimitiveValue { get; set; }

        Types IPrimitiveInstance.Type
        {
            get { return Types.Boolean; }
        }

        JsValue IPrimitiveInstance.PrimitiveValue
        {
            get { return PrimitiveValue; }
        }
    }
}