using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;

namespace FastReflection
{
    public class FastProperty<T, P>
    {
        public PropertyInfo Property { get; set; }

        public Func<T, P> GetDelegate;
        public Action<T, P> SetDelegate;

        public FastProperty(PropertyInfo property)
        {
            this.Property = property;
            InitializeGet();
            InitializeSet();

            
        }

        private void InitializeSet()
        {
            var instance = Expression.Parameter(typeof(T), "instance");
            var value = Expression.Parameter(typeof(P), "value");
            this.SetDelegate = Expression.Lambda<Action<T, P>>(Expression.Call(instance, this.Property.GetSetMethod(), value), new ParameterExpression[] { instance, value }).Compile();

            // roughly looks like Action<T,P> a = new Action<T,P>((instance,value) => instance.set_Property(value));
        }

        private void InitializeGet()
        {
            var instance = Expression.Parameter(typeof(T), "instance");
            this.GetDelegate = Expression.Lambda<Func<T, P>>(Expression.Call(instance, this.Property.GetGetMethod()), instance).Compile();

            // roughly looks like Func<T,P> getter = instance => return instance.get_Property();
        }

        public P Get(T instance)
        {
            return this.GetDelegate(instance);
        }

        public void Set(T instance, P value)
        {
            this.SetDelegate(instance, value);
        }
    }
}
