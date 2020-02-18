# SqlPlus

**SqlPlus** is an automatic parameterized **SQL** authoring tool.

- [English Readme](https://github.com/zmjack/SqlPlus/blob/master/README.md)
- [中文自述](https://github.com/zmjack/SqlPlus/blob/master/README-CN.md)

<br/>

## 使用说明

### 0. Refer to the database provider

All the examples in this article are described using **Sqlite**, using the following database provider:

- [Microsoft.Data.Sqlite](https://www.nuget.org/packages/Microsoft.Data.Sqlite)

Other database providers:

- **SQLite**: [Microsoft.Data.Sqlite](https://www.nuget.org/packages/Microsoft.Data.Sqlite)
- **MySql**: [MySqlConnector](https://www.nuget.org/packages/MySqlConnector)
- **SqlServer**: [Microsoft.EntityFrameworkCore.SqlServer](https://www.nuget.org/packages/Microsoft.EntityFrameworkCore.SqlServer)

<br/>

### 1. Build accessor

Build the data accessor from **SqlScope**:

```c#
public class ApplicationDbScope
    : SqlScope<ApplicationDbScope, SqliteConnection, SqliteCommand, SqliteParameter>
{
    public const string CONNECT_STRING = "filename=sqlplus.db";
    public static ApplicationDbScope UseDefault()
    	=> new ApplicationDbScope(new SqliteConnection(CONNECT_STRING));

    public ApplicationDbScope(SqliteConnection model) : base(model) { }
}
```

<br/>

### 2. Query with no return records

Use the **Sql** method for automatic parameterized query with no return records.

For example, insert data using the following statement:

```c#
using (var sqlite = ApplicationDbScope.UseDefault())
{
    sqlite.Sql($"INSERT INTO main (CreationTime, Integer, Real, Text) VALUES ({creationTime}, {416L}, {5.21d}, {"Hello"});");
}
```

The following **SQL** statement with parameters will be generated for query:

```sqlite
INSERT INTO main (CreationTime, Integer, Real, Text) VALUES (@p0, @p1, @p2, @p3);
```

If you need to monitor the execution of **SQL**, you can register the **OnExcuted** event:

```c#
using (var sqlite = ApplicationDbScope.UseDefault())
{
    var output = new StringBuilder();
    sqlite.OnExecuted += command => output.AppendLine(command.CommandText);

    sqlite.Sql($"INSERT INTO main (CreationTime, Integer, Real, Text) VALUES ({creationTime}, {416L}, {5.21d}, {"Hello"});");

    Assert.Equal(@"INSERT INTO main (CreationTime, Integer, Real, Text) VALUES (@p0, @p1, @p2, @p3);
", output.ToString());
}
```

<br/>

### 3. Query with return records

Use the **SqlQuery** method for automatic parameterized query with return records.

For example, query records in the table **main**, find the first record which **Text** is ***Hello*** (parameterized), return its **Real** value:

```c#
var record = sqlite.SqlQuery($"SELECT * FROM main WHERE Text={"Hello"};").First();
Assert.Equal(5.21d, record["Real"]);
```

<br/>

## Other tips

Automatic parameterization should be used to convert all unreliable inputs to <font color=red>prevent **SQL injection**</font>.

For example, query records in the table **main**, find the first record which **Text** is ***Hello*** (parameterized):

```c#
var text = "Hello";
sqlite.SqlQuery($"SELECT * FROM main WHERE Text={text};");
```

```sqlite
SELECT * FROM main WHERE Text=@p0;
/*
	@p0 = "Hello";
*/
```

<br/>

### SQL Injection

<font color=red>You should never use splicing to combine SQL statements</font>:

```c#
var text = "Hello";
sqlite.UnsafeSqlQuery("SELECT * FROM main WHERE Text='" + text +"';");
```

```sqlite
SELECT * FROM main WHERE Text='Hello';
```

In this way, the SQL semantics may be changed when user enters a specific value:

```c#
var text = "'or 1 or '";
sqlite.UnsafeSqlQuery("SELECT * FROM main WHERE Text='" + text +"';");
```

```sqlite
SELECT * FROM main WHERE Text='' or 1 or '';
```

This will lead to a series of security problems.
