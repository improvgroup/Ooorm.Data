namespace Ooorm.Data
{
    internal interface IDatabaseManagementQueryProvider
    {
        string DropDatabaseSql(string name);
        string DatabaseSql(string name);
    }
}
