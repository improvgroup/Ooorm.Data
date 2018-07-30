# Ooorm.Data

Define and manage your data layer from dotnet code

## "Birds eye view" example

Connect to a sql database (in this case, Sql Server)
```
using Ooorm.Data;
using Ooorm.Data.SqlServer;

var connectionSource = SqlServerConnectionSource.CreateSharedSource("Server=localhost;Database=master;Integrated Security=True;");

var database = new SqlDatabase(connectionSource);

await database.CreateDatabase("MyDb");

// creating a database connected to the new db
var db = new SqlDatabase(SqlServerConnectionSource.CreateSharedSource("Server=localhost;Database=MyDb;Integrated Security=True;"));

```

Create a table
```
class Person : IDbItem // implement IDbItem
{
    [Id]
    [Column]
    public int ID { get; set; }

    [Column]
    public string Name { get; set; }

    [Column]
    public string FavoritePizza { get; set; }
}


db.CreateTable<Person>();
```

CRUD operations
```

var bob = new Person{ Name = "Bob", FavoritePizza = "Veggie" };
db.Write(bob, new Person { Name = "Sally", FavoritePizza = "Greek" }, new Person { Name = "Soup", FavoritePizza = "Cheese" });

var sally = (await db.Read<Person>((row, name) => row.Name == name), "Sally").Single();

sally.FavoritePizza = "Taco";

await db.Update(sally);

await db.Delete(bob.ID);

var allPeople = await db.Read<Person>(); // [ { name: "Sally", favoritePizza: "Taco" }, { name: "Soup", favoritePizza: "Cheese" } ]
```