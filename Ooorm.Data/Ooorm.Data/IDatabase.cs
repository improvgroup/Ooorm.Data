using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Ooorm.Data
{
    public interface IDatabase : ISchema
    {
        /// <summary>
        /// Write values to the database
        /// </summary>
        Task<int> Write<T>(params T[] values) where T : IDbItem;

        /// <summary>
        /// Read all values from a table
        /// </summary>
        Task<IEnumerable<T>> Read<T>() where T : IDbItem;

        /// <summary>
        /// Read an item with the specified ID
        /// </summary>
        Task<T> Read<T>(int id) where T : IDbItem;

        /// <summary>
        /// Read items that amtch the parameterless predicate
        /// </summary>
        Task<IEnumerable<T>> Read<T>(Expression<Func<T, bool>> predicate) where T : IDbItem;

        /// <summary>
        /// Read items that match the predicate given the specified parameter
        /// </summary>
        Task<IEnumerable<T>> Read<T, TParam>(Expression<Func<T, TParam, bool>> predicate, TParam param) where T : IDbItem;

        /// <summary>
        /// Update the values of existing items
        /// </summary>
        Task<int> Update<T>(params T[] values) where T : IDbItem;

        /// <summary>
        /// Delete the items with the specified IDs
        /// </summary>
        Task<int> Delete<T>(params T[] values) where T : IDbItem;

        /// <summary>
        /// Delete the items that match the parameterless predicate
        /// </summary>
        Task<int> Delete<T>(Expression<Func<T, bool>> predicate) where T : IDbItem;

        /// <summary>
        /// Delete the items that
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TParam"></typeparam>
        /// <param name="predicate"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        Task<int> Delete<T, TParam>(Expression<Func<T, TParam, bool>> predicate, TParam param) where T : IDbItem;

        Task<T> Dereference<T>(DbVal<T> value) where T : IDbItem;
        Task<(bool exists, T value)> Dereference<T>(DbRef<T> value) where T : IDbItem;
    }
}
