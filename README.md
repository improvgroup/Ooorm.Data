# Ooorm.Data

Work in progress relation DB access library for C#.

##Goals: (listed aproximately in order of achievement level)
 * "Thin" abstraction layer, maintain the same relation representation of your data in your database and your entity classes, rather than implementing a document database on top of a relational one
 * Fluent api design to make reading db access code easier 
 * Strongly typed code-first query and schema generation, so that you can confidently use IDE refactoring tools
 * Chosen database is irrelevant to consuming code (adapters currently implemented for Sqlite and MS Sql Server)
 * Simple db mocking for tests with in-memory database that supports all library features
 [Future]
 * Tooling for automatically generating migration projects and web api projects from entity model assemblies

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
class Person : DbItem<Person, int> // implement IDbItem
{    
    public string Name { get; set; }
    public string FavoritePizza { get; set; }
}

await Person.CreateTableIn(db);
```

Create, read, update, and delete a record
```
// create a new record and write it to a database
var bob = await new Person{ Name = "Bob", FavoritePizza = "Veggie" }.WriteTo(db);

var bobFromDb = await Person.ReadById(bob.ID).From(db);
var allPeople = await Person.ReadAllFrom(db);

// expression converted to parameterized query
var veggiePizzaEaters = await Person.Read((row, pizza) => row.FavoritePizza == pizza).With("Veggie").From(db);

// update the record
bob.FavoritePizza = "Taco";
await bob.Update(db);

// delete the record
await bob.Delete(bob.ID);
```
