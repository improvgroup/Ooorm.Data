using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Ooorm.Data
{
    public interface ICrudRepository<T> where T : IDbItem
    {
        Task<int> Create(params T[] values);
        Task<IEnumerable<T>> Read();
        Task<IEnumerable<T>> Read(Expression<Func<T, bool>> predicate);
        Task<IEnumerable<T>> Read<TParam>(Expression<Func<T, TParam, bool>> predicate, TParam param);
        Task<int> Update(params T[] values);
        Task<int> Delete(params int[] ids);
    }
}
