using System;
using System.Diagnostics;
using System.Reflection;

namespace Ooorm.Data.Reflection
{
    public class Property
    {
        public readonly PropertyInfo Info;
        public readonly Type PropertyType;
        public readonly string PropertyName;
        public readonly Type ModelType;

        public Property(PropertyInfo property)
        {
            Info = property;
            PropertyType = property.PropertyType;
            PropertyName = property.Name;
            ModelType = property.DeclaringType;
            setter = (m, value) => property.SetMethod.Invoke(m, new object[] { value });
            //setter = (Action<object, object>)property.SetMethod.CreateDelegate(typeof(Action<object, object>));
            getter = (m) => property.GetMethod.Invoke(m, null);
            //getter = (Func<object, object>)property.GetMethod.CreateDelegate(typeof(Func<object, object>));
        }

        private readonly Action<object, object> setter;
        public virtual void SetOn(object model, object value)
        {
            if (value == null && (!PropertyType.IsValueType || IsNullable(PropertyType)))
            {
                setter(model, null);
            }
            else
            {
                //Debug.Assert(model.GetType().IsEquivalentTo(ModelType), $"Type of {nameof(model)} passed to {nameof(SetOn)} must match value of {nameof(ModelType)}.");
                //if (!(PropertyType.IsGenericType && PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>) && PropertyType == typeof(Nullable<>).MakeGenericType(value.GetType())))
                //    Debug.Assert(value.GetType().IsEquivalentTo(PropertyType), $"Type of {nameof(value)} passed to {nameof(SetOn)} must match value of {nameof(PropertyType)}.");
                setter(model, value);
            }
        }

        public bool IsNullable(Type type) => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);

        private readonly Func<object, object> getter;
        public virtual object GetFrom(object model) => getter(model);


        public bool IsDefaultOn(object model)
        {
            var self = GetFrom(model);
            var other = PropertyType.IsValueType ? Activator.CreateInstance(PropertyType) : null;
            return self?.Equals(other) ?? true;
        }
    }

    public class Column : Property
    {
        public readonly string ColumnName;

        public Column(PropertyInfo property) : base(property)
        {
            if (property.TryGetAttribute(out ColumnAttribute column))
                ColumnName = column.Value;
            else
                ColumnName = property.Name;
        }
    }

    public class Property<TModel> : Property
    {
        public Property(PropertyInfo property) : base(property)
        {
            var set = property.SetMethod.CreateDelegate(typeof(Action<,>).MakeGenericType(typeof(TModel), property.PropertyType));
            setter = (m, v) => set.DynamicInvoke(m, v);
            var get = property.GetMethod.CreateDelegate(typeof(Func<,>).MakeGenericType(typeof(TModel), property.PropertyType));
            getter = m => get.DynamicInvoke(m);
        }

        private readonly Action<TModel, object> setter;
        public void SetOn(TModel model, object value) => setter(model, value);


        private readonly Func<TModel, object> getter;
        public object GetFrom(TModel model) => getter(model);
    }

    public class Column<TModel> : Property<TModel>
    {
        public readonly string ColumnName;

        public Column(PropertyInfo property) : base(property)
        {
            if (property.TryGetAttribute(out ColumnAttribute column))
                ColumnName = column.Value;
            else
                ColumnName = property.Name;
        }
    }

    public class Property<TModel, TValue> : Property<TModel>
    {
        public Property(PropertyInfo property) : base(property)
        {
            setter = (Action<TModel, TValue>)property.SetMethod.CreateDelegate(typeof(Action<TModel, TValue>));
            getter = (Func<TModel, TValue>)property.GetMethod.CreateDelegate(typeof(Func<TModel, TValue>));
        }

        private readonly Action<TModel, TValue> setter;
        public void SetOn(TModel model, TValue value) => setter(model, value);


        private readonly Func<TModel, TValue> getter;
        public new TValue GetFrom(TModel model) => getter(model);
    }

    public class Column<TModel, TValue> : Property<TModel>
    {
        public readonly string ColumnName;

        public Column(PropertyInfo property) : base(property)
        {
            if (property.TryGetAttribute(out ColumnAttribute column))
                ColumnName = column.Value;
            else
                ColumnName = property.Name;
        }
    }
}
