using System;

namespace Ooorm.Data
{
    public interface ICrudRepository<T> where T : IDbItem
    {
    }
}
