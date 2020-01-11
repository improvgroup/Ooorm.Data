﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Ooorm.Data
{
    public interface ICrudRepository
    {
        Task<int> CreateTable();
        Task<int> DropTable();
        Task<IEnumerable<object>> ReadUntyped();
    }

    public interface ICrudRepository<T, TId> : ICrudRepository where T : IDbItem<TId> where TId : struct, IEquatable<TId>
    {
        Task<int> Write(params T[] values);
        Task<IEnumerable<T>> Read();
        Task<T> Read(int id);
        Task<IEnumerable<T>> Read(Expression<Func<T, bool>> predicate);
        Task<IEnumerable<T>> Read<TParam>(Expression<Func<T, TParam, bool>> predicate, TParam param);
        Task<int> Update(params T[] values);
        Task<int> Delete(params T[] values);
        Task<int> Delete(Expression<Func<T, bool>> predicate);
        Task<int> Delete<TParam>(Expression<Func<T, TParam, bool>> predicate, TParam param);
    }
}
