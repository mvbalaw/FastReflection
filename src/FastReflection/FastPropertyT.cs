using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;

namespace FastReflection
{
	public class FastProperty<T>
	{
		public PropertyInfo Property { get; set; }

		public Func<T, object> GetDelegate;
		public Action<T, object> SetDelegate;

		public FastProperty(PropertyInfo property)
		{
			this.Property = property;
			InitializeGet();
			InitializeSet();
		}

		private void InitializeSet()
		{
			var setMethod = this.Property.GetSetMethod();
			if (setMethod != null)
			{
				var instance = Expression.Parameter(typeof(T), "instance");
				var value = Expression.Parameter(typeof(object), "value");
				UnaryExpression valueCast = (!this.Property.PropertyType.IsValueType)
												? Expression.TypeAs(value, this.Property.PropertyType)
												: Expression.Convert(value, this.Property.PropertyType);
				this.SetDelegate =
					Expression.Lambda<Action<T, object>>(Expression.Call(instance, setMethod, valueCast),
														 new ParameterExpression[] { instance, value }).Compile();
			}
		}

		private void InitializeGet()
		{
			var getMethod = this.Property.GetGetMethod();
			if (getMethod != null)
			{
				var instance = Expression.Parameter(typeof(T), "instance");
				this.GetDelegate =
					Expression.Lambda<Func<T, object>>(Expression.TypeAs(Expression.Call(instance, getMethod), typeof(object)),
													   instance).Compile();
			}
		}

		public object Get(T instance)
		{
			return this.GetDelegate(instance);
		}

		public bool CanWrite
		{
			get { return SetDelegate != null; }
		}

		public void Set(T instance, object value)
		{
			this.SetDelegate(instance, value);
		}


	}
}
