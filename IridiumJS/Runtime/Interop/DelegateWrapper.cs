using System;
using System.Globalization;
using System.Linq;
using IridiumJS.Native;
using IridiumJS.Native.Function;

namespace IridiumJS.Runtime.Interop
{
    /// <summary>
    ///     Represents a FunctionInstance wrapper around a CLR method. This is used by user to pass
    ///     custom methods to the engine.
    /// </summary>
    public sealed class DelegateWrapper : FunctionInstance
    {
        private readonly Delegate _d;

        public DelegateWrapper(IridiumJSEngine engine, Delegate d) : base(engine, null, null, false)
        {
            _d = d;
        }

        public override JsValue Call(JsValue thisObject, JsValue[] jsArguments)
        {
            var parameterInfos = _d.Method.GetParameters();

            var delegateContainsParamsArgument =
                parameterInfos.Any(p => Attribute.IsDefined(p, typeof(ParamArrayAttribute)));
            var delegateArgumentsCount = parameterInfos.Length;
            var delegateNonParamsArgumentsCount = delegateContainsParamsArgument
                ? delegateArgumentsCount - 1
                : delegateArgumentsCount;

            var jsArgumentsCount = jsArguments.Length;
            var jsArgumentsWithoutParamsCount = Math.Min(jsArgumentsCount, delegateNonParamsArgumentsCount);

            var parameters = new object[delegateArgumentsCount];

            // convert non params parameter to expected types
            for (var i = 0; i < jsArgumentsWithoutParamsCount; i++)
            {
                var parameterType = parameterInfos[i].ParameterType;

                if (parameterType == typeof(JsValue))
                {
                    parameters[i] = jsArguments[i];
                }
                else
                {
                    parameters[i] = Engine.ClrTypeConverter.Convert(
                        jsArguments[i].ToObject(),
                        parameterType,
                        CultureInfo.InvariantCulture);
                }
            }

            // assign null to parameters not provided
            for (var i = jsArgumentsWithoutParamsCount; i < delegateNonParamsArgumentsCount; i++)
            {
                if (parameterInfos[i].ParameterType.IsValueType)
                {
                    parameters[i] = Activator.CreateInstance(parameterInfos[i].ParameterType);
                }
                else
                {
                    parameters[i] = null;
                }
            }

            // assign params to array and converts each objet to expected type
            if (delegateContainsParamsArgument)
            {
                var paramsArgumentIndex = delegateArgumentsCount - 1;
                var paramsCount = Math.Max(0, jsArgumentsCount - delegateNonParamsArgumentsCount);

                var paramsParameter = new object[paramsCount];
                var paramsParameterType = parameterInfos[paramsArgumentIndex].ParameterType.GetElementType();

                for (var i = paramsArgumentIndex; i < jsArgumentsCount; i++)
                {
                    var paramsIndex = i - paramsArgumentIndex;

                    if (paramsParameterType == typeof(JsValue))
                    {
                        paramsParameter[paramsIndex] = jsArguments[i];
                    }
                    else
                    {
                        paramsParameter[paramsIndex] = Engine.ClrTypeConverter.Convert(
                            jsArguments[i].ToObject(),
                            paramsParameterType,
                            CultureInfo.InvariantCulture);
                    }
                }
                parameters[paramsArgumentIndex] = paramsParameter;
            }

            return JsValue.FromObject(Engine, _d.DynamicInvoke(parameters));
        }
    }
}