﻿using System;
using IridiumJS.Native.Object;
using IridiumJS.Runtime;

namespace IridiumJS.Native.Number
{
    public class NumberInstance : ObjectInstance, IPrimitiveInstance
    {
        private static readonly long NegativeZeroBits = BitConverter.DoubleToInt64Bits(-0.0);

        public NumberInstance(Engine engine)
            : base(engine)
        {
        }

        public override string Class
        {
            get { return "Number"; }
        }

        public JsValue PrimitiveValue { get; set; }

        Types IPrimitiveInstance.Type
        {
            get { return Types.Number; }
        }

        JsValue IPrimitiveInstance.PrimitiveValue
        {
            get { return PrimitiveValue; }
        }

        public static bool IsNegativeZero(double x)
        {
            return x == 0 && BitConverter.DoubleToInt64Bits(x) == NegativeZeroBits;
        }

        public static bool IsPositiveZero(double x)
        {
            return x == 0 && BitConverter.DoubleToInt64Bits(x) != NegativeZeroBits;
        }
    }
}