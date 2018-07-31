# Ooorm.Data

Define and manage your data layer from dotnet code

## Example

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
// classes implemeting IDbItem define tables
class Person : IDbItem // implement IDbItem
{    
    public int ID { get; set; } 
    public string Name { get; set; }
    public string FavoritePizza { get; set; }
}


await db.CreateTable<Person>();
```

Create, read, update, and delete a record
```
var bob = new Person{ Name = "Bob", FavoritePizza = "Veggie" };

await db.Write(bob);

var bobFromDb = await db.Read<Person>(bob.ID);
var allPeople = await db.Read<Person>();

// expression converted to parameterized query
var veggiePizzaEaters = await db.Read<Person>((row, pizza) => row.FavoritePizza == pizza, "Veggie");


bob.FavoritePizza = "Taco";

await db.Update(bob);

await db.Delete<Person>(bob.ID);
```