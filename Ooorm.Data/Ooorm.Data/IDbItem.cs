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
        public static async Task CreateTableIn(this Type type, IDatabase db)
            => await db.CreateTables(type);

        public static async Task<T> WriteTo<T>(this T item, IDatabase db) where T : IDbItem
        {
            var rows = item.ID == default ? await db.Write(item) : await db.Update(item);
            return item;
        }

        public static async Task<int> DeleteMatchingFrom<T>(this T item, IDatabase db = null) where T : IDbItem
            => item.ID != default ? await db.Delete<T>(item.ID) : await db.Delete(item.MatchingPredicate());

        public static async Task<IEnumerable<T>> ReadMatchingFrom<T>(this T item, IDatabase db = null) where T : IDbItem
            => await db.Read(MatchingPredicate(item));


        public static Expression<Func<T, bool>> MatchingPredicate<T>(this T item) where T : IDbItem
        {
            var p = Expression.Parameter(typeof(T), "row");
            var matches = MatchExpressions(item, p);
            if (!matches.Any())
                return (Expression<Func<T, bool>>)Expression.Lambda(Expression.Equal(Expression.Constant(1), Expression.Constant(1)), p);
            var last = matches.First();
            foreach (var exp in matches.Skip(1))
                last = Expression.AndAlso(last, exp);
            return (Expression<Func<T, bool>>)Expression.Lambda(last, p);
        }

        private static IEnumerable<BinaryExpression> MatchExpressions<T>(this T item, ParameterExpression p) where T : IDbItem
        {
            foreach (var column in item.GetColumns().Where(c => !c.IsDefaultOn(item)))
                yield return
                    Expression.Equal(
                        Expression.MakeMemberAccess(p, column.Info),
                        Expression.Constant(column.GetFrom(item), column.PropertyType));
        }


    }
}
