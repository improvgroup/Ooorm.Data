using System;

namespace Ooorm.Data
{
    public class Param<T, TId> : DbItem<Param<T, TId>, TId> where TId : struct, IEquatable<TId>
    {
        public T Value { get; set; }        
    }
}
