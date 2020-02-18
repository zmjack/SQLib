# SqlPlus

**SqlPlus** 是一个自动参数化的 **SQL** 创作工具。

- [English Readme](https://github.com/zmjack/SqlPlus/blob/master/README.md)
- [中文自述](https://github.com/zmjack/SqlPlus/blob/master/README-CN.md)

<br/>

## 使用说明

### 0. 引用数据库提供程序

本文中的所有示例都使用 **Sqlite** 进行描述，使用的数据库提供程序为：

- [Microsoft.Data.Sqlite](https://www.nuget.org/packages/Microsoft.Data.Sqlite)

其他数据库提供程序：

- **SQLite**: [Microsoft.Data.Sqlite](https://www.nuget.org/packages/Microsoft.Data.Sqlite)
- **MySql**: [MySqlConnector](https://www.nuget.org/packages/MySqlConnector)
- **SqlServer**: [Microsoft.EntityFrameworkCore.SqlServer](https://www.nuget.org/packages/Microsoft.EntityFrameworkCore.SqlServer)

<br/>

### 1. 构建访问器

从 **SqlScope** 构建数据访问器：

```c#
public class ApplicationDbScope : SqlScope<ApplicationDbScope, SqliteConnection, SqliteCommand, SqliteParameter>
{
    public const string CONNECT_STRING = "filename=sqlplus.db";
    public static ApplicationDbScope UseDefault() => new ApplicationDbScope(new SqliteConnection(CONNECT_STRING));

    public ApplicationDbScope(SqliteConnection model) : base(model) { }
}
```
<br/>

### 2. 无返回记录查询

使用 **Sql** 方法进行无返回记录的自动参数化查询。

例如，使用如下语句进行数据插入：

```c#
using (var sqlite = ApplicationDbScope.UseDefault())
{
    sqlite.Sql($"INSERT INTO main (CreationTime, Integer, Real, Text) VALUES ({creationTime}, {416L}, {5.21d}, {"Hello"});");
}
```

将生成以下带有参数的 **SQL** 语句用于查询：

```sqlite
INSERT INTO main (CreationTime, Integer, Real, Text) VALUES (@p0, @p1, @p2, @p3);
```

如果需要监视 **SQL** 的执行，可以注册 **OnExcuted** 事件

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

### 3. 有返回记录查询

使用 **SqlQuery** 进行有返回记录的自动参数化查询。

例如，在 **main** 表中查询记录，找到 **Text** 是 ***Hello*** （参数化）的首条记录，返回它的 **Real** 值:

```c#
var record = sqlite.SqlQuery($"SELECT * FROM main WHERE Text={"Hello"};").First();
Assert.Equal(5.21d, record["Real"]);
```

<br/>

## 其他提示

应该使用自动参数化来转换所有不可靠的输入，以<font color=red>防止 **SQL 注入**</font>。

例如，查询 **main** 表中 **Text** 为 ***Hello***（用户输入）的第一条记录：

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

### SQL 注入

<font color=red>永远不应该使用拼接方式来组合 SQL 语句</font>：

```c#
var text = "Hello";
sqlite.UnsafeSqlQuery("SELECT * FROM main WHERE Text='" + text +"';");
```

```sqlite
SELECT * FROM main WHERE Text='Hello';
```

如果这样做，当用户输入特定值时，**SQL** 语义可能会改变：

```c#
var text = "'or 1 or '";
sqlite.UnsafeSqlQuery("SELECT * FROM main WHERE Text='" + text +"';");
```

```sqlite
SELECT * FROM main WHERE Text='' or 1 or '';
```

这将导致一系列安全问题。

