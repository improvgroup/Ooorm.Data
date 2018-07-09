using Ooorm.Data.Reflection;
using System;
using System.Data;

namespace Ooorm.Data
{
    public interface ITypeProvider
    {
        DbType DbType<TClrType>();
        DbType DbType(Type clrType);
        Type ClrType(DbType dbType);
        string DbTypeString(Column column);
    }
}
