namespace Ooorm.Data.Schemas
{
    internal interface ISchemaProvider
    {
        string DatabaseSql(string name);
        string TableSql<TModel>(ITypeResolver types) where TModel : IDbItem;
    }
}
