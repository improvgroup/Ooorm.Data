using System;
using System.Data;
using Ooorm.Data.Reflection;

namespace Ooorm.Data.SqlServer
{
    /// <summary>
    /// Provides type translation maps for Microsoft Sql Server
    /// </summary>
    internal class SqlServerTypeProvider : ExtendableTypeProvider
    {        
        public SqlServerTypeProvider(Func<IDatabase> db) : base(db) { }     
    }
}
