using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Ooorm.Data
{
    public interface IDatabase : ISchema
    {
        Task<int> Write<T>(params T[] values) where T : IDbItem;
        Task<IEnumerable<T>> Read<T>() where T : IDbItem;
        Task<T> Read<T>(int id) where T : IDbItem;
        Task<IEnumerable<T>> Read<T>(Expression<Func<T, bool>> predicate) where T : IDbItem;
        Task<IEnumerable<T>> Read<T, TParam>(Expression<Func<T, TParam, bool>> predicate, TParam param) where T : IDbItem;
        Task<int> Update<T>(params T[] values) where T : IDbItem;
        Task<int> Delete<T>(params int[] ids) where T : IDbItem;
        Task<int> Delete<T>(Expression<Func<T, bool>> predicate) where T : IDbItem;
        Task<int> Delete<T, TParam>(Expression<Func<T, TParam, bool>> predicate, TParam param) where T : IDbItem;

        Task<T> Dereference<T>(DbVal<T> value) where T : IDbItem;
        Task<(bool exists, T value)> Dereference<T>(DbRef<T> value) where T : IDbItem;
    }
}
