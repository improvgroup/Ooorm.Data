namespace Ooorm.Data.Core
{
    public class Param<T> : IDbItem
    {
        public T Value { get; set; }
        public int ID { get; set; }
    }
}
