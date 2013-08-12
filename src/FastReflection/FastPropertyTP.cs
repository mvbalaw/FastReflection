using System;
using System.Linq.Expressions;
using System.Reflection;

namespace FastReflection
{
	public class FastProperty<T, TP>
	{
		private Func<T, TP> _getDelegate;
		private Action<T, TP> _setDelegate;

		public FastProperty(PropertyInfo property)
		{
			Property = property;
			CanRead = property.GetGetMethod() != null;
			CanWrite = property.GetSetMethod() != null;
			_getDelegate = t =>
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

		public TP Get(T instance)
		{
			return _getDelegate(instance);
		}

		private void InitializeGet()
		{
			var getMethod = Property.GetGetMethod();
			if (getMethod != null)
			{
				var instance = Expression.Parameter(typeof(T), "instance");
				_getDelegate = Expression.Lambda<Func<T, TP>>(Expression.Call(instance, getMethod), instance).Compile();
			}
			// roughly looks like Func<T,P> getter = instance => return instance.get_Property();
		}

		private void InitializeSet()
		{
			var setMethod = Property.GetSetMethod();
			if (setMethod != null)
			{
				var instance = Expression.Parameter(typeof(T), "instance");
				var value = Expression.Parameter(typeof(TP), "value");
				_setDelegate =
					Expression.Lambda<Action<T, TP>>(Expression.Call(instance, setMethod, value),
					                                new[] { instance, value }).Compile();
			}
			// roughly looks like Action<T,P> a = new Action<T,P>((instance,value) => instance.set_Property(value));
		}

		public void Set(T instance, TP value)
		{
			_setDelegate(instance, value);
		}
	}
}