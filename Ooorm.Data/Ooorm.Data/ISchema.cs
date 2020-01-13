using System;
using System.Threading.Tasks;

namespace Ooorm.Data
{
    public interface ISchema
    {
        Task CreateTable<T, TId>() where T : DbItem<T, TId> where TId : struct, IEquatable<TId>;        
        Task DropTable<T, TId>() where T : DbItem<T, TId> where TId : struct, IEquatable<TId>;        
    }
}
