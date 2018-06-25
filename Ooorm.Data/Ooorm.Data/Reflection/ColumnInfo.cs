using Ooorm.Data.Attributes;
using System;
using System.Diagnostics;
using System.Reflection;

namespace Ooorm.Data.Reflection
{
    public class ColumnInfo
    {
        protected readonly PropertyInfo property;

        public readonly Type ModelType;
        public readonly Type ClrType;
        public readonly string ColumnName;
        public readonly string PropertyName;

        public ColumnInfo(PropertyInfo property)
        {
            this.property = property;
            setter = (Action<object, object>)property.SetMethod.CreateDelegate(typeof(Action<object, object>));
            getter = (Func<object, object>)property.GetMethod.CreateDelegate(typeof(Func<object, object>));
            ModelType = property.DeclaringType;
            ClrType = property.PropertyType;
            ColumnName = property.GetCustomAttribute<ColumnAttribute>().Value;
            PropertyName = property.Name;
        }

        private readonly Action<object, object> setter;
        public virtual void SetOn(object model, object value)
        {
            Debug.Assert(model.GetType().IsEquivalentTo(ModelType), $"Type of {nameof(model)} passed to {nameof(SetOn)} must match value of {nameof(ModelType)}.");
            Debug.Assert(value.GetType().IsEquivalentTo(ClrType), $"Type of {nameof(value)} passed to {nameof(SetOn)} must match value of {nameof(ClrType)}.");
            setter(model, value);
        }

        private readonly Func<object, object> getter;
        public virtual object GetFrom(object model)
        {
            Debug.Assert(model.GetType().IsEquivalentTo(ModelType), $"Type of {nameof(model)} passed to {nameof(GetFrom)} must match value of {nameof(ModelType)}.");
            return getter(model);
        }
    }

    public class ColumnInfo<TModel> : ColumnInfo
    {
        public ColumnInfo(PropertyInfo property) : base(property)
        {
            setter = (Action<TModel, object>)property.SetMethod.CreateDelegate(typeof(Action<TModel, object>));
            getter = (Func<TModel, object>)property.GetMethod.CreateDelegate(typeof(Func<TModel, object>));
        }

        private readonly Action<TModel, object> setter;
        public void SetOn(TModel model, object value)
        {
            Debug.Assert(value.GetType().IsEquivalentTo(ClrType), $"Type of {nameof(value)} passed to {nameof(SetOn)} must match value of {nameof(ClrType)}.");
            setter(model, value);
        }

        private readonly Func<TModel, object> getter;
        public object GetFrom(TModel model) => getter(model);
    }

    public class ColumnInfo<TModel, TValue> : ColumnInfo<TModel>
    {
        public ColumnInfo(PropertyInfo property) : base(property)
        {
            setter = (Action<TModel, TValue>)property.SetMethod.CreateDelegate(typeof(Action<TModel, TValue>));
            getter = (Func<TModel, TValue>)property.GetMethod.CreateDelegate(typeof(Func<TModel, TValue>));
        }

        private readonly Action<TModel, TValue> setter;
        public void SetOn(TModel model, TValue value) => setter(model, value);


        private readonly Func<TModel, TValue> getter;
        public new TValue GetFrom(TModel model) => getter(model);
    }
}
