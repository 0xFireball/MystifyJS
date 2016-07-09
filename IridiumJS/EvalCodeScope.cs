using System;

namespace IridiumJS
{
    public class EvalCodeScope : IDisposable
    {
        [ThreadStatic] private static int _refCount;

        private readonly bool _eval;
        private readonly bool _force;
        private readonly int _forcedRefCount;

        public EvalCodeScope(bool eval = true, bool force = false)
        {
            _eval = eval;
            _force = force;

            if (_force)
            {
                _forcedRefCount = _refCount;
                _refCount = 0;
            }

            if (_eval)
            {
                _refCount++;
            }
        }

        public static bool IsEvalCode
        {
            get { return _refCount > 0; }
        }

        public static int RefCount
        {
            get { return _refCount; }
            set { _refCount = value; }
        }

        public void Dispose()
        {
            if (_eval)
            {
                _refCount--;
            }

            if (_force)
            {
                _refCount = _forcedRefCount;
            }
        }
    }
}