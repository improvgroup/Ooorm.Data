namespace Ooorm.Data
{
    public interface IDbItem
    {
        int ID { get; set; }
    }

    public interface IdConvertable<TId>
    {
        TId ToId();
    }
}
