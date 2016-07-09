﻿using System.Globalization;
using System.Reflection;
using IridiumJS.Native;

namespace IridiumJS.Runtime.Descriptors.Specialized
{
    public sealed class PropertyInfoDescriptor : PropertyDescriptor
    {
        private readonly Engine _engine;
        private readonly object _item;
        private readonly PropertyInfo _propertyInfo;

        public PropertyInfoDescriptor(Engine engine, PropertyInfo propertyInfo, object item)
        {
            _engine = engine;
            _propertyInfo = propertyInfo;
            _item = item;

            Writable = propertyInfo.CanWrite;
        }

        public override JsValue? Value
        {
            get { return JsValue.FromObject(_engine, _propertyInfo.GetValue(_item, null)); }

            set
            {
                var currentValue = value.GetValueOrDefault();
                object obj;
                if (_propertyInfo.PropertyType == typeof(JsValue))
                {
                    obj = currentValue;
                }
                else
                {
                    // attempt to convert the JsValue to the target type
                    obj = currentValue.ToObject();
                    if (obj != null && obj.GetType() != _propertyInfo.PropertyType)
                    {
                        obj = _engine.ClrTypeConverter.Convert(obj, _propertyInfo.PropertyType,
                            CultureInfo.InvariantCulture);
                    }
                }

                _propertyInfo.SetValue(_item, obj, null);
            }
        }
    }
}