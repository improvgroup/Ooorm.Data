using System;

namespace Ooorm.Data
{
    public class DbInfoAttribute : Attribute
    {
        public readonly string Value;
        public DbInfoAttribute(string value) => Value = value;
    }

    public class TableAttribute : DbInfoAttribute
    {
        public TableAttribute(string name) : base(name) { }
    }

    public class ColumnAttribute : DbInfoAttribute
    {
        public ColumnAttribute(string column) : base(column) { }
    }
}
