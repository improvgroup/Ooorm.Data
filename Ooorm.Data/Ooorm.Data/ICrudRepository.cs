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
        Task<List<object>> ReadUntyped();
    }

    public interface ICrudRepository<T, TId> : ICrudRepository where T : DbItem<T, TId> where TId : struct, IEquatable<TId>
    {
        Task<SortedList<TId, T>> Write(params T[] values);
        Task<List<T>> Read();
        Task<T> Read(TId id);
        Task<List<T>> Read(Expression<Func<T, bool>> predicate);
        Task<List<T>> Read<TParam>(Expression<Func<T, TParam, bool>> predicate, TParam param);
        Task<SortedList<TId, T>> Update(params T[] values);
        Task<int> Delete(params T[] values);
        Task<int> Delete(Expression<Func<T, bool>> predicate);
        Task<int> Delete<TParam>(Expression<Func<T, TParam, bool>> predicate, TParam param);
    }
}
