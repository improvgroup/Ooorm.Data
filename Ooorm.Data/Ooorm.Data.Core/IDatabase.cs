using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Ooorm.Data.Core
{
    public interface IReadable
    {
        IAsyncEnumerable<T> Read<T>() where T : IDbItem;
        IAsyncEnumerable<object> Read(Type type);
        Task<T> Read<T>(int id) where T : IDbItem;
        IAsyncEnumerable<T> Read<T>(Expression<Func<T, bool>> predicate) where T : IDbItem;
        IAsyncEnumerable<T> Read<T, TParam>(Expression<Func<T, TParam, bool>> predicate, TParam param) where T : IDbItem;
        Task<T> Dereference<T>(DbVal<T> value) where T : IDbItem;
        Task<(bool exists, T value)> Dereference<T>(DbRef<T> value) where T : IDbItem;
    }

    public interface IWritable
    {        
        Task<int> Write<T>(params T[] values) where T : IDbItem;
        
        Task<int> Update<T>(params T[] values) where T : IDbItem;
        
        Task<int> Delete<T>(params T[] values) where T : IDbItem;
        
        Task<int> Delete<T>(Expression<Func<T, bool>> predicate) where T : IDbItem;
        
        Task<int> Delete<T, TParam>(Expression<Func<T, TParam, bool>> predicate, TParam param) where T : IDbItem;
    }

    public interface ISchema
    {
        Task CreateTable<T>() where T : IDbItem;
        Task CreateTables(params Type[] tables);
        Task DropTable<T>() where T : IDbItem;
        Task DropTables(params Type[] tables);
    }


    public interface IDatabase : IReadable, IWritable, ISchema
    {

    }
}
