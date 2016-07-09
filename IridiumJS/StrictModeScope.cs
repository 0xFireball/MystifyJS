﻿using System;

namespace IridiumJS
{
    public class StrictModeScope : IDisposable
    {
        [ThreadStatic] private static int _refCount;

        private readonly bool _force;
        private readonly int _forcedRefCount;
        private readonly bool _strict;

        public StrictModeScope(bool strict = true, bool force = false)
        {
            _strict = strict;
            _force = force;

            if (_force)
            {
                _forcedRefCount = _refCount;
                _refCount = 0;
            }

            if (_strict)
            {
                _refCount++;
            }
        }

        public static bool IsStrictModeCode
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
            if (_strict)
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