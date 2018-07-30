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
        object ToDbValue(object value);
        object FromDbValue(object value, Type type);
        bool IsDbValueType(Type clrType);
    }
}
