using System;
using System.Linq.Expressions;
using System.Reflection;

namespace FastReflection
{
	public class FastProperty
	{
		private Func<object, object> _getDelegate;
		private Action<object, object> _setDelegate;

		public FastProperty(PropertyInfo property)
		{
			Property = property;
			CanRead = property.GetGetMethod() != null;
			CanWrite = property.GetSetMethod() != null;
			_getDelegate = (t) =>
				{
					InitializeGet();
					return _getDelegate(t);
				};
			_setDelegate = (t, p) =>
				{
					InitializeSet();
					_setDelegate(t, p);
				};
		}

		public bool CanRead { get; private set; }
		public bool CanWrite { get; private set; }
		public PropertyInfo Property { get; private set; }

		public object Get(object instance)
		{
			return _getDelegate(instance);
		}

		private void InitializeGet()
		{
			var getMethod = Property.GetGetMethod();
			if (getMethod != null)
			{
				var instance = Expression.Parameter(typeof(object), "instance");
				var instanceCast = (!Property.DeclaringType.IsValueType)
				                   	? Expression.TypeAs(instance, Property.DeclaringType)
				                   	: Expression.Convert(instance, Property.DeclaringType);
				_getDelegate =
					Expression.Lambda<Func<object, object>>(
						Expression.TypeAs(Expression.Call(instanceCast, getMethod), typeof(object)), instance).Compile();
			}
		}

		private void InitializeSet()
		{
			var setMethod = Property.GetSetMethod();
			if (setMethod != null)
			{
				var instance = Expression.Parameter(typeof(object), "instance");
				var value = Expression.Parameter(typeof(object), "value");

				// value as T is slightly faster than (T)value, so if it's not a value type, use that
				var instanceCast = (!Property.DeclaringType.IsValueType)
				                   	? Expression.TypeAs(instance, Property.DeclaringType)
				                   	: Expression.Convert(instance, Property.DeclaringType);
				var valueCast = (!Property.PropertyType.IsValueType)
				                	? Expression.TypeAs(value, Property.PropertyType)
				                	: Expression.Convert(value, Property.PropertyType);
				_setDelegate =
					Expression.Lambda<Action<object, object>>(Expression.Call(instanceCast, setMethod, valueCast),
					                                          new[] { instance, value }).Compile();
			}
		}

		public void Set(object instance, object value)
		{
			_setDelegate(instance, value);
		}
	}
}