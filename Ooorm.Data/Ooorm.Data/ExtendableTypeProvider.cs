using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Ooorm.Data.Reflection;
using Ooorm.Data.TypeResolvers;

namespace Ooorm.Data
{
    public interface IExtendableTypeResolver
    {
        DbType GetDbType(Column column);
        string GetDbTypeString(Column column);
        object DbSerialize(Type type, object value);
        object DbDeserialize(Type type, object value);
        bool IsDbValueType(Type type);
        bool IsReference(Type type);
    }

    internal class ExtendableTypeProvider : IExtendableTypeResolver
    {
        private readonly Func<IDatabase> database;

        private readonly Dictionary<Type, TypeHandler> _baseHandlers = new Dictionary<Type, TypeHandler>();

        private readonly Dictionary<Type, TypeHandler> _providedHandlers = new Dictionary<Type, TypeHandler>();

        private void RegisterBaseHandler<TClr, TDb>(TypeHandler<TClr, TDb> handler) => _baseHandlers[typeof(TClr)] = handler;
        

        protected void RegisterHandler<TClr, TDb>(TypeHandler<TClr, TDb> handler) => _providedHandlers[typeof(TClr)] = handler;

        protected TypeHandler GetHandler(Type type) =>
            _providedHandlers.ContainsKey(type) ? _providedHandlers[type] : _baseHandlers[type];        

        public ExtendableTypeProvider(Func<IDatabase> db)
        {
            this.database = db;
            RegisterBaseHandler(new BooleanHandler());   
            RegisterBaseHandler(new Int8Handler());
            RegisterBaseHandler(new UInt8Handler());
            RegisterBaseHandler(new Int16Handler());
            RegisterBaseHandler(new UInt16Handler());
            RegisterBaseHandler(new Int32Handler());
            RegisterBaseHandler(new UInt32Handler());
            RegisterBaseHandler(new Int64Handler());
            RegisterBaseHandler(new UInt64Handler());
            RegisterBaseHandler(new FloatHandler());
            RegisterBaseHandler(new DoubleHandler());
            RegisterBaseHandler(new DecimalHandler());
            RegisterBaseHandler(new StringHandler());
            RegisterBaseHandler(new GuidHandler());
            RegisterBaseHandler(new DateTimeHandler());
            RegisterBaseHandler(new DateTimeOffsetHandler());
            RegisterBaseHandler(new ByteArrayHandler());
        }

        public object DbDeserialize(Type type, object value) /*=> GetHandler(type).DeserializeObject(value);*/
        {
            if (value == DBNull.Value)
                return null;
            if (type.IsEnum)
                return Enum.ToObject(type, value);
            if (IsDbVal(type))
                return Activator.CreateInstance(
                    typeof(DbVal<,>).MakeGenericType(type.GenericTypeArguments), value, database);
            else if (IsDbRef(type))
                return Activator.CreateInstance(
                    typeof(DbRef<,>).MakeGenericType(type.GenericTypeArguments), value, database);
            else if (IsNullable(type, out Type generic))
            {
                try { return GetHandler(generic).DeserializeObject(value); }
                catch (Exception) { return null; }
            }
            else
                return GetHandler(type).DeserializeObject(value);
        }

        public object DbSerialize(Type type, object value) /*=> GetHandler(type).SerializeObject(value);*/
        {
            if (value == null)
                return DBNull.Value;
            else if (value.GetType().IsEnum)
                return (int)value;
            else if (value is IdConvertable<int> valId)
                return valId.ToId();
            else if (value is IdConvertable<int?> refId)
                return refId.ToId();
            else if (IsNullable(type, out Type generic))
                return GetHandler(generic).SerializeObject(value);
            else
                return GetHandler(type).SerializeObject(value);
        }

        public DbType GetDbType(Column column) => GetHandler(UnwrapColumnType(column)).GetDbType(column);

        public string GetDbTypeString(Column column) => GetHandler(UnwrapColumnType(column)).GetDbTypeString(column);

        private Type UnwrapColumnType(Column column)
        {
            if (HasMapping(column.PropertyType))
                return column.PropertyType;
            else if (IsNullable(column.PropertyType, out Type generic) && HasMapping(generic))
                return generic;
            else if (IsDbVal(column.PropertyType) || IsDbRef(column.PropertyType))
                return typeof(int);
            else if (column.PropertyType.IsEnum)
                return column.PropertyType.GetEnumUnderlyingType();
            else
                throw new InvalidOperationException($"Cannot translate clr type {column.PropertyType} to a database type");
        }

        private bool IsNullable(Type clrType, out Type generic)
            => (clrType.IsGenericType && clrType.GetGenericTypeDefinition() == typeof(Nullable<>)) ? (generic = clrType.GenericTypeArguments.FirstOrDefault()) != null : (generic = null) != null;

        private bool IsDbVal(Type clrType)
            => clrType.IsGenericType && clrType.GetGenericTypeDefinition() == typeof(DbVal<,>);

        private bool IsDbRef(Type clrType)
            => clrType.IsGenericType && clrType.GetGenericTypeDefinition() == typeof(DbRef<,>);

        public bool IsReference(Type clrType) => IsDbVal(clrType) || IsDbRef(clrType);

        public bool IsDbValueType(Type clrType)
            => (clrType.IsGenericType && (clrType == typeof(DbRef<,>).MakeGenericType(clrType.GenericTypeArguments) || clrType == typeof(DbVal<,>).MakeGenericType(clrType.GenericTypeArguments)))
                    || HasMapping(clrType);

        private bool HasMapping(Type clrType)
            => _baseHandlers.ContainsKey(clrType) || _providedHandlers.ContainsKey(clrType);
    }
}

