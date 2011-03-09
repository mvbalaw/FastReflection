using System;
using System.Linq.Expressions;
using System.Reflection;

namespace FastReflection
{
	public class FastProperty<T>
	{
		private Func<T, object> _getDelegate;
		private Action<T, object> _setDelegate;

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

		public object Get(T instance)
		{
			return _getDelegate(instance);
		}

		private void InitializeGet()
		{
			var getMethod = Property.GetGetMethod();
			if (getMethod != null)
			{
				var instance = Expression.Parameter(typeof(T), "instance");
				_getDelegate =
					Expression.Lambda<Func<T, object>>(Expression.TypeAs(Expression.Call(instance, getMethod), typeof(object)),
					                                   instance).Compile();
			}
		}

		private void InitializeSet()
		{
			var setMethod = Property.GetSetMethod();
			if (setMethod != null)
			{
				var instance = Expression.Parameter(typeof(T), "instance");
				var value = Expression.Parameter(typeof(object), "value");
				var valueCast = (!Property.PropertyType.IsValueType)
				                	? Expression.TypeAs(value, Property.PropertyType)
				                	: Expression.Convert(value, Property.PropertyType);
				_setDelegate =
					Expression.Lambda<Action<T, object>>(Expression.Call(instance, setMethod, valueCast),
					                                     new[] { instance, value }).Compile();
			}
		}

		public void Set(T instance, object value)
		{
			_setDelegate(instance, value);
		}
	}
}