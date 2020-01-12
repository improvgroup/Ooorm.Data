using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Ooorm.Data.Sqlite
{
    public class SqliteDatabase : IDatabase
    {        
        private readonly SqliteDao dao;

        private readonly Dictionary<Type, object> repositories = new Dictionary<Type, object>();

        private ICrudRepository<T, TId> Repos<T, TId>() where T : DbItem<T, TId> where TId : struct, IEquatable<TId> =>
            (ICrudRepository<T, TId>)(repositories.ContainsKey(typeof(T)) ? repositories[typeof(T)] : (repositories[typeof(T)] = new SqliteRepository<T, TId>(source, () => this)));

        private ICrudRepository Repos(Type type)
        {
            var id_type = type.GetProperty(nameof(Param<int,int>.ID)).GetType();
            var repo_type = typeof(SqliteRepository<,>).MakeGenericType(type, id_type);
            Console.WriteLine(id_type);
            return (ICrudRepository)(repositories.ContainsKey(type) 
                    ? repositories[type] 
                    : (repositories[type] = Activator.CreateInstance(repo_type, source, (Func<IDatabase>)(() => this))));
        }

        private readonly SqliteConnection source;

        public SqliteDatabase(SqliteConnection source) => (this.source, dao) = (source, new SqliteDao(() => this));

        public async Task<SortedList<TId, T>> Write<T, TId>(params T[] values) where T : DbItem<T, TId> where TId : struct, IEquatable<TId>
            => await Repos<T, TId>().Write(values);

        public async Task<int> Delete<T, TId>(params T[] values) where T : DbItem<T, TId> where TId : struct, IEquatable<TId>
            => await Repos<T, TId>().Delete(values);

        public async Task<int> Delete<T, TId>(Expression<Func<T, bool>> predicate) where T : DbItem<T, TId> where TId : struct, IEquatable<TId>
            => await Repos<T, TId>().Delete(predicate);

        public async Task<int> Delete<T, TParam, TId>(Expression<Func<T, TParam, bool>> predicate, TParam param) where T : DbItem<T, TId> where TId : struct, IEquatable<TId>
            => await Repos<T, TId>().Delete(predicate, param);

        public async Task<List<T>> Read<T, TId>() where T : DbItem<T, TId> where TId : struct, IEquatable<TId>
            => await Repos<T, TId>().Read();

        public async Task<T> Read<T, TId>(TId id) where T : DbItem<T, TId> where TId : struct, IEquatable<TId>
            => await Repos<T, TId>().Read(id);

        public async Task<List<T>> Read<T, TId>(Expression<Func<T, bool>> predicate) where T : DbItem<T, TId> where TId : struct, IEquatable<TId>
            => await Repos<T, TId>().Read(predicate);

        public async Task<List<T>> Read<T, TParam, TId>(Expression<Func<T, TParam, bool>> predicate, TParam param) where T : DbItem<T, TId> where TId : struct, IEquatable<TId>
            => await Repos<T, TId>().Read(predicate, param);

        public async Task<List<object>> Read(Type type)
            => await Repos(type).ReadUntyped();

        public async Task<SortedList<TId, T>> Update<T, TId>(params T[] values) where T : DbItem<T, TId> where TId : struct, IEquatable<TId>
            => await Repos<T, TId>().Update(values);

        public async Task<T> Dereference<T, TId>(DbVal<T, TId> value) where T : DbItem<T, TId> where TId : struct, IEquatable<TId>
            => await Repos<T, TId>().Read(value);

        public async Task<(bool exists, T value)> Dereference<T, TId>(DbRef<T, TId> value) where T : DbItem<T, TId> where TId : struct, IEquatable<TId>
            => value.IsNull ? (false, default) : (true, await Repos<T, TId>().Read(((TId?)value).Value));

        public async Task CreateTable<T, TId>() where T : DbItem<T, TId> where TId : struct, IEquatable<TId>
            => await Repos<T, TId>().CreateTable();

        public async Task DropTable<T, TId>() where T : DbItem<T, TId> where TId : struct, IEquatable<TId>
            => await Repos<T, TId>().DropTable();

        public async Task CreateTables(params Type[] tables)
        {
            foreach (var type in tables)
                await Repos(type).CreateTable();
        }

        public async Task DropTables(params Type[] tables)
        {
            foreach (var type in tables)
                await Repos(type).DropTable();
        }
    }
}
