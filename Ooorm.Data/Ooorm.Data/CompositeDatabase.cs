using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Ooorm.Data
{
    public class CompositeDatabase : IDatabase
    {
        private readonly IDatabase front;
        private readonly IDatabase back;

        private readonly HashSet<Type> backTypes = new HashSet<Type>();

        public CompositeDatabase(IDatabase frontDb, IDatabase backDb)
        {
            (front, back) = (frontDb, backDb);
        }

        public void AddBackTypes(params Type[] types)
        {
            foreach (var type in types)
                backTypes.Add(type);
        }

        public async Task CreateTable<T, TId>() where T : DbItem<T, TId> where TId : struct, IEquatable<TId>
        {
            if (backTypes.Contains(typeof(T)))
                await back.CreateTable<T, TId>();
            else
                await front.CreateTable<T, TId>();
        }

        public async Task<int> Delete<T, TId>(params T[] values) where T : DbItem<T, TId> where TId : struct, IEquatable<TId>
        {
            if (backTypes.Contains(typeof(T)))
                return await back.Delete<T, TId>(values);
            else
                return await front.Delete<T, TId>(values);
        }

        public async Task<int> Delete<T, TId>(Expression<Func<T, bool>> predicate) where T : DbItem<T, TId> where TId : struct, IEquatable<TId>
        {
            if (backTypes.Contains(typeof(T)))
                return await back.Delete<T, TId>(predicate);
            else
                return await front.Delete<T, TId>(predicate);
        }

        public async Task<int> Delete<T, TParam, TId>(Expression<Func<T, TParam, bool>> predicate, TParam param) where T : DbItem<T, TId> where TId : struct, IEquatable<TId>
        {
            if (backTypes.Contains(typeof(T)))
                return await back.Delete<T, TParam, TId>(predicate, param);
            else
                return await front.Delete<T, TParam, TId>(predicate, param);
        }

        public Task<int> Delete<T, TId>(Expression<Func<T>> constructor)
            where T : DbItem<T, TId>
            where TId : struct, IEquatable<TId>
        {
            if (backTypes.Contains(typeof(T)))
                return back.Delete<T, TId>(constructor);
            else
                return front.Delete<T, TId>(constructor);
        }

        public async Task<T> Dereference<T, TId>(DbVal<T, TId> value) where T : DbItem<T, TId> where TId : struct, IEquatable<TId>
        {
            if (backTypes.Contains(typeof(T)))
                return await back.Dereference(value);
            else
                return await front.Dereference(value);
        }

        public async Task<(bool exists, T value)> Dereference<T, TId>(DbRef<T, TId> value) where T : DbItem<T, TId> where TId : struct, IEquatable<TId>
        {
            if (backTypes.Contains(typeof(T)))
                return await back.Dereference(value);
            else
                return await front.Dereference(value);
        }

        public async Task DropTable<T, TId>() where T : DbItem<T, TId> where TId : struct, IEquatable<TId>
        {
            if (backTypes.Contains(typeof(T)))
                await back.DropTable<T, TId>();
            else
                await front.DropTable<T, TId>();
        }

        public async Task<List<T>> Read<T, TId>() where T : DbItem<T, TId> where TId : struct, IEquatable<TId>
        {
            if (backTypes.Contains(typeof(T)))
                return await back.Read<T, TId>();
            else
                return await front.Read<T, TId>();
        }

        public async Task<List<T>> Read<T, TId>(Expression<Func<T, bool>> predicate) where T : DbItem<T, TId> where TId : struct, IEquatable<TId>
        {
            if (backTypes.Contains(typeof(T)))
                return await back.Read<T, TId>(predicate);
            else
                return await front.Read<T, TId>(predicate);
        }

        public async Task<List<T>> Read<T, TParam, TId>(Expression<Func<T, TParam, bool>> predicate, TParam param) where T : DbItem<T, TId> where TId : struct, IEquatable<TId>
        {
            if (backTypes.Contains(typeof(T)))
                return await back.Read<T, TParam, TId>(predicate, param);
            else
                return await front.Read<T, TParam, TId>(predicate, param);
        }

        public async Task<T> Read<T, TId>(TId id) where T : DbItem<T, TId> where TId : struct, IEquatable<TId>
        {
            if (backTypes.Contains(typeof(T)))
                return await back.Read<T, TId>(id);
            else
                return await front.Read<T, TId>(id);
        }

        public Task<List<T>> Read<T, TId>(Expression<Func<T>> constructor)
            where T : DbItem<T, TId>
            where TId : struct, IEquatable<TId>        
        {
            if (backTypes.Contains(typeof(T)))
                return back.Read<T, TId>(constructor);
            else
                return front.Read<T, TId>(constructor);
        }       

        public async Task<SortedList<TId, T>> Update<T, TId>(params T[] values) where T : DbItem<T, TId> where TId : struct, IEquatable<TId>
        {
            if (backTypes.Contains(typeof(T)))
                return await back.Update<T, TId>(values);
            else
                return await front.Update<T, TId>(values);
        }

        public async Task<SortedList<TId, T>> Write<T, TId>(params T[] values) where T : DbItem<T, TId> where TId : struct, IEquatable<TId>
        {
            if (backTypes.Contains(typeof(T)))
                return await back.Write<T, TId>(values);
            else
                return await front.Write<T, TId>(values);
        }
    }
}
