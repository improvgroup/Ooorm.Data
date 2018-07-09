using System;
using System.Data;
using Ooorm.Data.Reflection;

namespace Ooorm.Data.SqlServer
{
    /// <summary>
    /// Provides type translation maps for Microsoft Sql Server
    /// </summary>
    internal class SqlServerTypeProvider : ITypeProvider
    {        
        private readonly DefaultTypeProvider defaults;

        public SqlServerTypeProvider()
        {
            defaults = new DefaultTypeProvider();
        }

        public Type ClrType(DbType dbType)
        {
            return defaults.ClrType(dbType);
        }

        public DbType DbType<TClrType>()
        {
            return defaults.DbType<TClrType>();
        }

        public DbType DbType(Type clrType)
        {
            return defaults.DbType(clrType);
        }

        public string DbTypeString(Column column)
        {
            return defaults.DbTypeString(column);
        }
    }
}
