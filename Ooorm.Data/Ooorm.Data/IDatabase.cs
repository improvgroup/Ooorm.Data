using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Ooorm.Data
{
    public interface IReadable
    {
        Task<IEnumerable<T>> Read<T, TId>() where T : IDbItem<TId> where TId : struct, IEquatable<TId>;

        Task<IEnumerable<object>> Read(Type type);

        Task<T> Read<T, TId>(TId id) where T : IDbItem<TId> where TId : struct, IEquatable<TId>;

        Task<IEnumerable<T>> Read<T, TId>(Expression<Func<T, bool>> predicate) where T : IDbItem<TId> where TId : struct, IEquatable<TId>;

        Task<IEnumerable<T>> Read<T, TParam, TId>(Expression<Func<T, TParam, bool>> predicate, TParam param) where T : IDbItem<TId> where TId : struct, IEquatable<TId>;

        Task<T> Dereference<T, TId>(DbVal<T, TId> value) where T : IDbItem<TId> where TId : struct, IEquatable<TId>;

        Task<(bool exists, T value)> Dereference<T, TId>(DbRef<T, TId> value) where T : IDbItem<TId> where TId : struct, IEquatable<TId>;
    }

    public interface IWritable
    {
        /// <summary>
        /// Write values to the database
        /// </summary>
        Task<int> Write<T, TId>(params T[] values) where T : IDbItem<TId> where TId : struct, IEquatable<TId>;

        /// <summary>
        /// Update the values of existing items
        /// </summary>
        Task<int> Update<T, TId>(params T[] values) where T : IDbItem<TId> where TId : struct, IEquatable<TId>;

        /// <summary>
        /// Delete the items with the specified IDs
        /// </summary>
        Task<int> Delete<T, TId>(params T[] values) where T : IDbItem<TId> where TId : struct, IEquatable<TId>;

        /// <summary>
        /// Delete the items that match the parameterless predicate
        /// </summary>
        Task<int> Delete<T, TId>(Expression<Func<T, bool>> predicate) where T : IDbItem<TId> where TId : struct, IEquatable<TId>;

        /// <summary>
        /// Delete the items that
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TParam"></typeparam>
        /// <param name="predicate"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        Task<int> Delete<T, TParam, TId>(Expression<Func<T, TParam, bool>> predicate, TParam param) where T : IDbItem<TId> where TId : struct, IEquatable<TId>;
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
