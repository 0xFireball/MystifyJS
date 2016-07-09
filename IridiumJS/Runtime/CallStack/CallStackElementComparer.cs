using System.Collections.Generic;

namespace IridiumJS.Runtime.CallStack
{
    public class CallStackElementComparer : IEqualityComparer<CallStackElement>
    {
        public bool Equals(CallStackElement x, CallStackElement y)
        {
            return x.Function == y.Function;
        }

        public int GetHashCode(CallStackElement obj)
        {
            return obj.Function.GetHashCode();
        }
    }
}