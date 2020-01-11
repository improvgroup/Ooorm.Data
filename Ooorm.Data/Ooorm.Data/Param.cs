using System;

namespace Ooorm.Data
{
    public class Param<T, TId> : IDbItem<TId> where TId : struct, IEquatable<TId>
    {
        public T Value { get; set; }        
    }
}
