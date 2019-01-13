using System.Collections.Generic;
using System.Data;
using Ooorm.Data.Core.Reflection;

namespace Ooorm.Data.Core
{
    internal interface IDataConsumer<TDataReader> where TDataReader : IDataReader
    {
        IEnumerable<string> Fields(TDataReader reader);
        object ReadColumn(TDataReader reader, Column column, int index, IExtendableTypeResolver types);
    }
}