namespace Ooorm.Data.Schemas
{
    public interface ISchemaProvider
    {
        string DatabaseSql(string name); 
        string TableSql<TModel>(ITypeProvider types) where TModel : IDbItem;        
    }
}
