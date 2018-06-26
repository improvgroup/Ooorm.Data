using System;
using System.Data;

namespace Ooorm.Data
{
    public interface ITypeProvider
    {
        DbType DbType<TClrType>();
        DbType DbType(Type clrType);
        Type ClrType(DbType dbType);

        TClrType ToClrValue<TClrType>(object dbValue);
        object ToDbValue<TClrType>(TClrType clrValue);
    }
}
