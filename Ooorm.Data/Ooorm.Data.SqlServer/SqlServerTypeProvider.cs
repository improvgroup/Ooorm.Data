using System;
using System.Data;
using Ooorm.Data.Reflection;

namespace Ooorm.Data.SqlServer
{
    /// <summary>
    /// Provides type translation maps for Microsoft Sql Server
    /// </summary>
    internal class SqlServerTypeProvider : ITypeResolver
    {
        private readonly DefaultTypeProvider defaults;

        public SqlServerTypeProvider(Func<IDatabase> db) => defaults = new DefaultTypeProvider(db);

        public Type ClrType(DbType dbType) => defaults.ClrType(dbType);

        public DbType DbType<TClrType>() => defaults.DbType<TClrType>();

        public DbType DbType(Type clrType) => defaults.DbType(clrType);

        public string DbTypeString(Column column) => defaults.DbTypeString(column);

        public object ToDbValue(object value) => defaults.ToDbValue(value);

        public object FromDbValue(object value, Type type) => defaults.FromDbValue(value, type);

        public bool IsDbValueType(Type clrType) => defaults.IsDbValueType(clrType);
    }
}
