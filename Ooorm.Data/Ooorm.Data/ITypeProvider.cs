using System;

namespace Ooorm.Data
{
    public interface ITypeProvider
    {
        string DbType<TClrType>();
        string DbType(Type clrType);
        string ClrType(string dbType);

        TClrType ToClrValue<TClrType>(object dbValue);
        object ToDbValue<TClrType>(TClrType clrValue);
    }
}
