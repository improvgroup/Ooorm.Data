using System.Collections.Generic;
using System.Data;
using Ooorm.Data.Reflection;

namespace Ooorm.Data
{
    public interface IDataConsumer<TDataReader> where TDataReader : IDataReader
    {
        IEnumerable<string> Fields(TDataReader reader);
        object ReadColumn(TDataReader reader, Column column, int index, ITypeProvider types);
    }
}