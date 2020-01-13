using Ooorm.Data.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Ooorm.Data
{
    public abstract class DbItem<TSelf, TId> where TId : struct, IEquatable<TId> where TSelf : DbItem<TSelf, TId>
    {
        internal bool IsNew { get; set; } = true;
        public TId ID { get; internal set; }
    
        public static implicit operator TSelf(DbItem<TSelf, TId> a) => (TSelf)a;

        /// <summary>
        /// Writes a db item to the specified database and returns the result
        /// </summary>
        public async Task<TSelf> WriteTo(IDatabase db) =>
            (this.IsNew ? (await db.Write<TSelf, TId>(this)) : (await db.Update<TSelf, TId>(this))).Single().Value;        

        /// <summary>
        /// Deletes all records from the db that match each non-default field in item
        /// </summary>
        /// <returns>Number of deleted records</returns>
        public Task<int> DeleteMatchingFrom(IDatabase db = null)
            => this.IsNew ? db.Delete<TSelf, TSelf, TId>(MatchingPredicate(), this) : db.Delete<TSelf, TId>(this);

        /// <summary>
        /// Reads all records from the db that match each non-default field in item
        /// </summary>
        /// <returns>Matching records</returns>
        public Task<List<TSelf>> ReadMatchingFrom(IDatabase db = null)
            => db.Read<TSelf, TSelf, TId>(MatchingPredicate(), this);

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
            foreach (var column in ((TSelf)this).GetColumns<TSelf, TId>(exceptId: true).Where(c => !c.IsDefaultOn(this)))
                yield return Expression.Equal(Expression.MakeMemberAccess(row, column.Info), Expression.MakeMemberAccess(p, column.Info));
        }

        public DbRef<TSelf, TId> In(IDatabase database) =>        
            IsNew ? new DbRef<TSelf, TId>(ID, () => database) : throw new KeyNotFoundException("Cannot add reference reference to an item without a DB");
        
        public static Task CreateTable(IDatabase database) => database.CreateTable<TSelf, TId>();

        public static Task<List<TSelf>> ReadFrom(IDatabase database) => database.Read<TSelf, TId>();

        public class FromOp<T>
        {
            private readonly Func<IDatabase, Task<T>> _operation;

            public FromOp(Func<IDatabase, Task<T>> operation) => _operation = operation;

            public Task<T> From(IDatabase db) => _operation(db);
        }

        public static FromOp<TSelf> ReadById(TId id) => new FromOp<TSelf>(db => db.Read<TSelf, TId>(id));

        public static FromOp<List<TSelf>> Read(Expression<Func<TSelf, bool>> predicate) => new FromOp<List<TSelf>>(db => db.Read<TSelf, TId>(predicate));

        public class ParamOp<TParam>
        {
            private readonly Expression<Func<TSelf, TParam, bool>> _predicate;
            
            public ParamOp(Expression<Func<TSelf, TParam, bool>> predicate) => _predicate = predicate;

            public FromOp<List<TSelf>> With(TParam param) => new FromOp<List<TSelf>>(db => db.Read<TSelf, TParam, TId>(_predicate, param));
        }

        public static ParamOp<TParam> Read<TParam>(Expression<Func<TSelf, TParam, bool>> predicate) => new ParamOp<TParam>(predicate);
        
    }    
}
