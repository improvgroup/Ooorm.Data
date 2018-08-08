using Ooorm.Data.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Ooorm.Data
{
    public interface IDbItem
    {
        int ID { get; set; }
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
        public static async Task<T> WriteTo<T>(this T item, IDatabase db) where T : IDbItem
        {
            var rows = item.ID == default ? await db.Write(item) : await db.Update(item);
            return item;
        }

        /// <summary>
        /// Deletes all records from the db that match each non-default field in item
        /// </summary>
        /// <returns>Number of deleted records</returns>
        public static async Task<int> DeleteMatchingFrom<T>(this T item, IDatabase db = null) where T : IDbItem
            => item.ID != default ? await db.Delete<T>(item.ID) : await db.Delete(item.MatchingPredicate(), item);

        /// <summary>
        /// Reads all records from the db that match each non-default field in item
        /// </summary>
        /// <returns>Matching records</returns>
        public static async Task<IEnumerable<T>> ReadMatchingFrom<T>(this T item, IDatabase db = null) where T : IDbItem
            => await db.Read(MatchingPredicate(item), item);

        /// <summary>
        /// Creates a query compatable predicate expression that matches all non-default fields of item
        /// </summary>
        public static Expression<Func<T, T, bool>> MatchingPredicate<T>(this T item) where T : IDbItem
        {
            var row = Expression.Parameter(typeof(T), "row");
            var p = Expression.Parameter(typeof(T), "p");
            var matches = MatchExpressions(item, row, p);
            if (!matches.Any())
                return (Expression<Func<T, T, bool>>)Expression.Lambda(Expression.Equal(Expression.Constant(1), Expression.Constant(1)), row, p);
            var last = matches.First();
            foreach (var exp in matches.Skip(1))
                last = Expression.AndAlso(last, exp);
            return (Expression<Func<T, T, bool>>)Expression.Lambda(last, row, p);
        }

        private static IEnumerable<BinaryExpression> MatchExpressions<T>(this T item, ParameterExpression row, ParameterExpression p) where T : IDbItem
        {
            foreach (var column in item.GetColumns().Where(c => !c.IsDefaultOn(item)))
                yield return Expression.Equal(Expression.MakeMemberAccess(row, column.Info), Expression.MakeMemberAccess(p, column.Info));
        }


    }
}
