using Ooorm.Data.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Ooorm.Data
{
    public abstract class IDbItem<TId> where TId : struct, IEquatable<TId>
    {
        internal bool IsNew { get; set; } = true;
        public TId ID { get; set; }
    }

    public static class IDbItemExtensions
    {
        /// <summary>
        /// Creates a table for the specified type if it doesn't exist
        /// </summary>
        /// <param name="type">A CLR Type implementing IDbItem</param>
        public static async Task CreateTableIn(this Type type, IDatabase db)
            => await db.CreateTables(type);

        /// <summary>
        /// Writes a db item to the specified database and returns the result
        /// </summary>
        public static async Task<T> WriteTo<T, TId>(this T item, IDatabase db) where T : IDbItem<TId> where TId : struct, IEquatable<TId>
        {
            var rows = item.IsNew ? await db.Write<T, TId>(item) : await db.Update<T, TId>(item);
            return item;
        }

        /// <summary>
        /// Deletes all records from the db that match each non-default field in item
        /// </summary>
        /// <returns>Number of deleted records</returns>
        public static async Task<int> DeleteMatchingFrom<T, TId>(this T item, IDatabase db = null) where T : IDbItem<TId> where TId : struct, IEquatable<TId>
            => item.IsNew ? await db.Delete<T, T, TId>(item.MatchingPredicate<T, TId>(), item) : await db.Delete<T, TId>(item);

        /// <summary>
        /// Reads all records from the db that match each non-default field in item
        /// </summary>
        /// <returns>Matching records</returns>
        public static async Task<IEnumerable<T>> ReadMatchingFrom<T, TId>(this T item, IDatabase db = null) where T : IDbItem<TId> where TId : struct, IEquatable<TId>
            => await db.Read<T, T, TId>(MatchingPredicate<T, TId>(item), item);

        /// <summary>
        /// Creates a query compatable predicate expression that matches all non-default fields of item
        /// </summary>
        public static Expression<Func<T, T, bool>> MatchingPredicate<T, TId>(this T item) where T : IDbItem<TId> where TId : struct, IEquatable<TId>
        {
            var row = Expression.Parameter(typeof(T), "row");
            var p = Expression.Parameter(typeof(T), "p");
            var matches = MatchExpressions<T, TId>(item, row, p);
            if (!matches.Any())
                return (Expression<Func<T, T, bool>>)Expression.Lambda(Expression.Equal(Expression.Constant(1), Expression.Constant(1)), row, p);
            var last = matches.First();
            foreach (var exp in matches.Skip(1))
                last = Expression.AndAlso(last, exp);
            return (Expression<Func<T, T, bool>>)Expression.Lambda(last, row, p);
        }

        private static IEnumerable<BinaryExpression> MatchExpressions<T, TId>(this T item, ParameterExpression row, ParameterExpression p) where T : IDbItem<TId> where TId : struct, IEquatable<TId>
        {
            foreach (var column in item.GetColumns<T, TId>().Where(c => !c.IsDefaultOn(item)))
                yield return Expression.Equal(Expression.MakeMemberAccess(row, column.Info), Expression.MakeMemberAccess(p, column.Info));
        }


    }
}
