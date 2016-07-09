using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using IridiumJS.Runtime.Interop;

namespace IridiumJS
{
    public class Options
    {
        private readonly List<IObjectConverter> _objectConverters = new List<IObjectConverter>();
        private List<Assembly> _lookupAssemblies = new List<Assembly>();

        internal bool _IsGlobalDiscarded { get; private set; }

        internal bool _IsStrict { get; private set; }

        internal bool _IsDebuggerStatementAllowed { get; private set; }

        internal bool _IsDebugMode { get; private set; }

        internal bool _IsClrAllowed { get; private set; }

        internal IList<Assembly> _LookupAssemblies => _lookupAssemblies;

        internal IEnumerable<IObjectConverter> _ObjectConverters => _objectConverters;

        internal int _MaxStatements { get; private set; }

        internal int _MaxRecursionDepth { get; private set; } = -1;

        internal TimeSpan _TimeoutInterval { get; private set; }

        internal CultureInfo _Culture { get; private set; } = CultureInfo.CurrentCulture;

        internal TimeZoneInfo _LocalTimeZone { get; private set; } = TimeZoneInfo.Local;

        /// <summary>
        ///     When called, doesn't initialize the global scope.
        ///     Can be useful in lightweight scripts for performance reason.
        /// </summary>
        public Options DiscardGlobal(bool discard = true)
        {
            _IsGlobalDiscarded = discard;
            return this;
        }

        /// <summary>
        ///     Run the script in strict mode.
        /// </summary>
        public Options Strict(bool strict = true)
        {
            _IsStrict = strict;
            return this;
        }

        /// <summary>
        ///     Allow the <code>debugger</code> statement to be called in a script.
        /// </summary>
        /// <remarks>
        ///     Because the <code>debugger</code> statement can start the
        ///     Visual Studio debugger, is it disabled by default
        /// </remarks>
        public Options AllowDebuggerStatement(bool allowDebuggerStatement = true)
        {
            _IsDebuggerStatementAllowed = allowDebuggerStatement;
            return this;
        }

        /// <summary>
        ///     Allow to run the script in debug mode.
        /// </summary>
        public Options DebugMode(bool debugMode = true)
        {
            _IsDebugMode = debugMode;
            return this;
        }

        /// <summary>
        ///     Adds a <see cref="IObjectConverter" /> instance to convert CLR types to <see cref="JsValue" />
        /// </summary>
        public Options AddObjectConverter(IObjectConverter objectConverter)
        {
            _objectConverters.Add(objectConverter);
            return this;
        }

        /// <summary>
        ///     Allows scripts to call CLR types directly like
        ///     <example>System.IO.File</example>
        /// </summary>
        public Options AllowClr(params Assembly[] assemblies)
        {
            _IsClrAllowed = true;
            _lookupAssemblies.AddRange(assemblies);
            _lookupAssemblies = _lookupAssemblies.Distinct().ToList();
            return this;
        }

        public Options MaxStatements(int maxStatements = 0)
        {
            _MaxStatements = maxStatements;
            return this;
        }

        public Options TimeoutInterval(TimeSpan timeoutInterval)
        {
            _TimeoutInterval = timeoutInterval;
            return this;
        }

        /// <summary>
        ///     Sets maximum allowed depth of recursion.
        /// </summary>
        /// <param name="maxRecursionDepth">
        ///     The allowed depth.
        ///     a) In case max depth is zero no recursion is allowed.
        ///     b) In case max depth is equal to n it means that in one scope function can be called no more than n times.
        /// </param>
        /// <returns>Options instance for fluent syntax</returns>
        public Options LimitRecursion(int maxRecursionDepth = 0)
        {
            _MaxRecursionDepth = maxRecursionDepth;
            return this;
        }

        public Options Culture(CultureInfo cultureInfo)
        {
            _Culture = cultureInfo;
            return this;
        }

        public Options LocalTimeZone(TimeZoneInfo timeZoneInfo)
        {
            _LocalTimeZone = timeZoneInfo;
            return this;
        }
    }
}