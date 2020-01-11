using Ooorm.Data.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Ooorm.Data
{
    public abstract class IDbItem<TSelf, TId> where TId : struct, IEquatable<TId> where TSelf : IDbItem<TSelf, TId>
    {
        internal bool IsNew { get; set; } = true;
        public TId ID { get; internal set; }
    
        public static implicit operator TSelf(IDbItem<TSelf, TId> a) => (TSelf)a;

        /// <summary>
        /// Writes a db item to the specified database and returns the result
        /// </summary>
        public async Task<TSelf> WriteTo(IDatabase db) =>
            (this.IsNew ? (await db.Write<TSelf, TId>(this)) : (await db.Update<TSelf, TId>(this))).Single().Value;        

        /// <summary>
        /// Deletes all records from the db that match each non-default field in item
        /// </summary>
        /// <returns>Number of deleted records</returns>
        public async Task<int> DeleteMatchingFrom(IDatabase db = null)
            => this.IsNew ? await db.Delete<TSelf, TSelf, TId>(MatchingPredicate(), this) : await db.Delete<TSelf, TId>(this);

        /// <summary>
        /// Reads all records from the db that match each non-default field in item
        /// </summary>
        /// <returns>Matching records</returns>
        public async Task<List<TSelf>> ReadMatchingFrom(IDatabase db = null)
            => await db.Read<TSelf, TSelf, TId>(MatchingPredicate(), this);

        /// <summary>
        /// Creates a query compatable predicate expression that matches all non-default fields of item
        /// </summary>
        internal Expression<Func<TSelf, TSelf, bool>> MatchingPredicate()
        {
            var row = Expression.Parameter(typeof(TSelf), "row");
            var p = Expression.Parameter(typeof(TSelf), "p");
            var matches = MatchExpressions(row, p);
            if (!matches.Any())
                return (Expression<Func<TSelf, TSelf, bool>>)Expression.Lambda(Expression.Equal(Expression.Constant(1), Expression.Constant(1)), row, p);
            var last = matches.First();
            foreach (var exp in matches.Skip(1))
                last = Expression.AndAlso(last, exp);
            return (Expression<Func<TSelf, TSelf, bool>>)Expression.Lambda(last, row, p);
        }

        private IEnumerable<BinaryExpression> MatchExpressions(ParameterExpression row, ParameterExpression p)
        {
            foreach (var column in ((TSelf)this).GetColumns<TSelf, TId>().Where(c => !c.IsDefaultOn(this)))
                yield return Expression.Equal(Expression.MakeMemberAccess(row, column.Info), Expression.MakeMemberAccess(p, column.Info));
        }

        public DbRef<TSelf, TId> In(IDatabase database) =>        
            this.IsNew ? new DbRef<TSelf, TId>(this.ID, () => database) : throw new KeyNotFoundException("Cannot add reference reference to an item without a DB");
        
    }    
}
