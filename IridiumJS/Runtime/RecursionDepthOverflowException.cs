﻿using System;
using Jint.Runtime.CallStack;

namespace Jint.Runtime
{
    public class RecursionDepthOverflowException : Exception
    {
        public RecursionDepthOverflowException(JintCallStack currentStack, string currentExpressionReference)
            : base("The recursion is forbidden by script host.")
        {
            CallExpressionReference = currentExpressionReference;

            CallChain = currentStack.ToString();
        }

        public string CallChain { get; private set; }

        public string CallExpressionReference { get; private set; }
    }
}