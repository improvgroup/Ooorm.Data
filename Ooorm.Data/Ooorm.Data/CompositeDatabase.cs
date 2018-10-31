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


        public async Task<int> Delete<T>(params T[] values) where T : IDbItem
        {
            if (backTypes.Contains(typeof(T)))
                return await back.Delete(values);
            else
                return await front.Delete(values);
        }

        public async Task<int> Delete<T>(Expression<Func<T, bool>> predicate) where T : IDbItem
        {
            if (backTypes.Contains(typeof(T)))
                return await back.Delete(predicate);
            else
                return await front.Delete(predicate);
        }

        public async Task<int> Delete<T, TParam>(Expression<Func<T, TParam, bool>> predicate, TParam param) where T : IDbItem
        {
            if (backTypes.Contains(typeof(T)))
                return await back.Delete(predicate, param);
            else
                return await front.Delete(predicate, param);
        }

        public async Task<T> Dereference<T>(DbVal<T> value) where T : IDbItem
        {
            if (backTypes.Contains(typeof(T)))
                return await back.Dereference(value);
            else
                return await front.Dereference(value);
        }

        public async Task<(bool exists, T value)> Dereference<T>(DbRef<T> value) where T : IDbItem
        {
            if (backTypes.Contains(typeof(T)))
                return await back.Dereference(value);
            else
                return await front.Dereference(value);
        }

        public async Task<IEnumerable<T>> Read<T>() where T : IDbItem
        {
            if (backTypes.Contains(typeof(T)))
                return await back.Read<T>();
            else
                return await front.Read<T>();
        }

        public async Task<IEnumerable<T>> Read<T>(Expression<Func<T, bool>> predicate) where T : IDbItem
        {
            if (backTypes.Contains(typeof(T)))
                return await back.Read<T>(predicate);
            else
                return await front.Read<T>(predicate);
        }

        public async Task<IEnumerable<T>> Read<T, TParam>(Expression<Func<T, TParam, bool>> predicate, TParam param) where T : IDbItem
        {
            if (backTypes.Contains(typeof(T)))
                return await back.Read(predicate, param);
            else
                return await front.Read(predicate, param);
        }

        public async Task<T> Read<T>(int id) where T : IDbItem
        {
            if (backTypes.Contains(typeof(T)))
                return await back.Read<T>(id);
            else
                return await front.Read<T>(id);
        }

        public async Task<IEnumerable<object>> Read(Type type)
        {
            if (backTypes.Contains(type))
                return await back.Read(type);
            else
                return await front.Read(type);
        }

        public async Task<int> Update<T>(params T[] values) where T : IDbItem
        {
            if (backTypes.Contains(typeof(T)))
                return await back.Update(values);
            else
                return await front.Update(values);
        }

        public async Task<int> Write<T>(params T[] values) where T : IDbItem
        {
            if (backTypes.Contains(typeof(T)))
                return await back.Write(values);
            else
                return await front.Write(values);
        }
    }
}
