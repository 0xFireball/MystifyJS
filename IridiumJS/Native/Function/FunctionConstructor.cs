﻿using System;
using System.Collections.Generic;
using System.Linq;
using IridiumJS.Native.Object;
using IridiumJS.Native.String;
using IridiumJS.Parser;
using IridiumJS.Parser.Ast;
using IridiumJS.Runtime;
using IridiumJS.Runtime.Environments;

namespace IridiumJS.Native.Function
{
    public sealed class FunctionConstructor : FunctionInstance, IConstructor
    {
        private FunctionInstance _throwTypeError;

        private FunctionConstructor(IridiumJSEngine engine) : base(engine, null, null, false)
        {
        }

        public FunctionPrototype PrototypeObject { get; private set; }

        public FunctionInstance ThrowTypeError
        {
            get
            {
                if (_throwTypeError != null)
                {
                    return _throwTypeError;
                }

                _throwTypeError = new ThrowTypeError(Engine);
                return _throwTypeError;
            }
        }

        public override JsValue Call(JsValue thisObject, JsValue[] arguments)
        {
            return Construct(arguments);
        }

        public ObjectInstance Construct(JsValue[] arguments)
        {
            var argCount = arguments.Length;
            var p = "";
            var body = "";

            if (argCount == 1)
            {
                body = TypeConverter.ToString(arguments[0]);
            }
            else if (argCount > 1)
            {
                var firstArg = arguments[0];
                p = TypeConverter.ToString(firstArg);
                for (var k = 1; k < argCount - 1; k++)
                {
                    var nextArg = arguments[k];
                    p += "," + TypeConverter.ToString(nextArg);
                }

                body = TypeConverter.ToString(arguments[argCount - 1]);
            }

            var parameters = ParseArgumentNames(p);
            var parser = new JavaScriptParser();
            FunctionExpression function;
            try
            {
                var functionExpression = "function(" + p + ") { " + body + "}";
                function = parser.ParseFunctionExpression(functionExpression);
            }
            catch (ParserException)
            {
                throw new JavaScriptException(Engine.SyntaxError);
            }

            // todo: check if there is not a way to use the FunctionExpression directly instead of creating a FunctionDeclaration
            var functionObject = new ScriptFunctionInstance(
                Engine,
                new FunctionDeclaration
                {
                    Type = SyntaxNodes.FunctionDeclaration,
                    Body = new BlockStatement
                    {
                        Type = SyntaxNodes.BlockStatement,
                        Body = new[] {function.Body}
                    },
                    Parameters = parameters.Select(x => new Identifier
                    {
                        Type = SyntaxNodes.Identifier,
                        Name = x
                    }).ToArray(),
                    FunctionDeclarations = function.FunctionDeclarations,
                    VariableDeclarations = function.VariableDeclarations
                },
                LexicalEnvironment.NewDeclarativeEnvironment(Engine, Engine.ExecutionContext.LexicalEnvironment),
                function.Strict
                ) {Extensible = true};

            return functionObject;
        }

        public static FunctionConstructor CreateFunctionConstructor(IridiumJSEngine engine)
        {
            var obj = new FunctionConstructor(engine);
            obj.Extensible = true;

            // The initial value of Function.prototype is the standard built-in Function prototype object
            obj.PrototypeObject = FunctionPrototype.CreatePrototypeObject(engine);

            // The value of the [[Prototype]] internal property of the Function constructor is the standard built-in Function prototype object 
            obj.Prototype = obj.PrototypeObject;

            obj.FastAddProperty("prototype", obj.PrototypeObject, false, false, false);

            obj.FastAddProperty("length", 1, false, false, false);

            return obj;
        }

        public void Configure()
        {
        }

        private string[] ParseArgumentNames(string parameterDeclaration)
        {
            var values = parameterDeclaration.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries);

            var newValues = new string[values.Length];
            for (var i = 0; i < values.Length; i++)
            {
                newValues[i] = StringPrototype.TrimEx(values[i]);
            }
            return newValues;
        }

        /// <summary>
        ///     http://www.ecma-international.org/ecma-262/5.1/#sec-13.2
        /// </summary>
        /// <param name="functionDeclaration"></param>
        /// <returns></returns>
        public FunctionInstance CreateFunctionObject(FunctionDeclaration functionDeclaration)
        {
            var functionObject = new ScriptFunctionInstance(
                Engine,
                functionDeclaration,
                LexicalEnvironment.NewDeclarativeEnvironment(Engine, Engine.ExecutionContext.LexicalEnvironment),
                functionDeclaration.Strict
                ) {Extensible = true};

            return functionObject;
        }

        public object Apply(JsValue thisObject, JsValue[] arguments)
        {
            if (arguments.Length != 2)
            {
                throw new ArgumentException("Apply has to be called with two arguments.");
            }

            var func = thisObject.TryCast<ICallable>();
            var thisArg = arguments[0];
            var argArray = arguments[1];

            if (func == null)
            {
                throw new JavaScriptException(Engine.TypeError);
            }

            if (argArray == Null.Instance || argArray == Undefined.Instance)
            {
                return func.Call(thisArg, Arguments.Empty);
            }

            var argArrayObj = argArray.TryCast<ObjectInstance>();
            if (argArrayObj == null)
            {
                throw new JavaScriptException(Engine.TypeError);
            }

            var len = argArrayObj.Get("length");
            var n = TypeConverter.ToUint32(len);
            var argList = new List<JsValue>();
            for (var index = 0; index < n; index++)
            {
                var indexName = index.ToString();
                var nextArg = argArrayObj.Get(indexName);
                argList.Add(nextArg);
            }
            return func.Call(thisArg, argList.ToArray());
        }
    }
}