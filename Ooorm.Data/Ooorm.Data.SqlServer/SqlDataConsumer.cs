using System;
using System.Data.SqlClient;
using System.IO;

namespace Ooorm.Data.SqlServer
{
    internal class SqlDataConsumer : BaseDataConsumer<SqlDataReader>
    {
        protected override bool TryGetStream(SqlDataReader reader, int ordinal, out Stream stream)
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
