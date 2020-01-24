using System;
using Microsoft.Data.Sqlite;
using System.IO;
using Ooorm.Data.Reflection;

namespace Ooorm.Data.Sqlite
{
    internal class SqliteDataConsumer : BaseDataConsumer<SqliteDataReader>
    {
        public override object ReadColumn(SqliteDataReader reader, Column column, int index, IExtendableTypeResolver types)
        {
            if (reader.IsDBNull(index))
                return null;
            switch (types.GetDbType(column))
            {
                case System.Data.DbType.DateTimeOffset:
                    var text = reader.GetString(index);
                    return DateTimeOffset.Parse(text);
                default:
                    return base.ReadColumn(reader, column, index, types);
            }
        }

        protected override bool TryGetStream(SqliteDataReader reader, int ordinal, out Stream stream)
        {
            try
            {
                stream = reader.GetStream(ordinal);
                return true;
            }
            catch (InvalidOperationException)
            {
                stream = default;
                return false;
            }
            catch (IndexOutOfRangeException)
            {
                stream = default;
                return false;
            }
            catch (InvalidCastException)
            {
                stream = default;
                return false;
            }
        }
    }
}
