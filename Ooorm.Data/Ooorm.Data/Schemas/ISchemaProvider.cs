namespace Ooorm.Data.Schemas
{
    internal interface ISchemaProvider
    {
        string DatabaseSql(string name);
        string TableSql<TModel>(ITypeProvider types) where TModel : IDbItem;
    }
}
