using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Ooorm.Data
{
    public interface IReadable
    {
        Task<List<T>> Read<T, TId>() where T : IDbItem<T, TId> where TId : struct, IEquatable<TId>;

        Task<List<object>> Read(Type type);

        Task<T> Read<T, TId>(TId id) where T : IDbItem<T, TId> where TId : struct, IEquatable<TId>;

        Task<List<T>> Read<T, TId>(Expression<Func<T, bool>> predicate) where T : IDbItem<T, TId> where TId : struct, IEquatable<TId>;

        Task<List<T>> Read<T, TParam, TId>(Expression<Func<T, TParam, bool>> predicate, TParam param) where T : IDbItem<T, TId> where TId : struct, IEquatable<TId>;

        Task<T> Dereference<T, TId>(DbVal<T, TId> value) where T : IDbItem<T, TId> where TId : struct, IEquatable<TId>;

        Task<(bool exists, T value)> Dereference<T, TId>(DbRef<T, TId> value) where T : IDbItem<T, TId> where TId : struct, IEquatable<TId>;
    }

    public interface IWritable
    {
        /// <summary>
        /// Write values to the database and return the set of new IDs
        /// </summary>
        Task<SortedList<TId, T>> Write<T, TId>(params T[] values) where T : IDbItem<T, TId> where TId : struct, IEquatable<TId>;

        /// <summary>
        /// Update the values of existing items and return the new values
        /// </summary>
        Task<SortedList<TId, T>> Update<T, TId>(params T[] values) where T : IDbItem<T, TId> where TId : struct, IEquatable<TId>;

        /// <summary>
        /// Delete the items with the specified IDs and return the number of rows effected
        /// </summary>
        Task<int> Delete<T, TId>(params T[] values) where T : IDbItem<T, TId> where TId : struct, IEquatable<TId>;

        /// <summary>
        /// Delete the items that match the parameterless predicate and return the number of rows effected
        /// </summary>
        Task<int> Delete<T, TId>(Expression<Func<T, bool>> predicate) where T : IDbItem<T, TId> where TId : struct, IEquatable<TId>;

        /// <summary>
        /// Delete the items that match the condition and return the number of rows effected
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TParam"></typeparam>
        /// <param name="predicate"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        Task<int> Delete<T, TParam, TId>(Expression<Func<T, TParam, bool>> predicate, TParam param) where T : IDbItem<T, TId> where TId : struct, IEquatable<TId>;
    }

    public interface IDatabaseManagementSystem : IDatabase
    {
        Task DropDatabase(string name);
        Task CreateDatabase(string name, params Type[] tables);
    }


    public interface IDatabase : IReadable, IWritable, ISchema
    {

    }
}
