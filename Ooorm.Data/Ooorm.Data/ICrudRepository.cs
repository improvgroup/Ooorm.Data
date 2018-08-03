using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Ooorm.Data
{
    public interface ICrudRepository
    {
        Task<int> CreateTable();
        Task<int> DropTable();
    }

    public interface ICrudRepository<T> : ICrudRepository where T : IDbItem
    {
        Task<int> Write(params T[] values);
        Task<IEnumerable<T>> Read();
        Task<T> Read(int id);
        Task<IEnumerable<T>> Read(Expression<Func<T, bool>> predicate);
        Task<IEnumerable<T>> Read<TParam>(Expression<Func<T, TParam, bool>> predicate, TParam param);
        Task<int> Update(params T[] values);
        Task<int> Delete(params int[] ids);
        Task<int> Delete(Expression<Func<T, bool>> predicate);
        Task<int> Delete<TParam>(Expression<Func<T, TParam, bool>> predicate, TParam param);
    }
}
