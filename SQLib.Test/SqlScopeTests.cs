using Microsoft.Data.Sqlite;
using NStandard;
using SQLib.Data.Test;
using SQLib.Test.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace SQLib.Test
{
    public class SqlScopeTests
    {
        [Fact]
        public void Test0()
        {
            var sqlList = new List<string>();
            void onExecuted(SqliteCommand command) => sqlList.Add(command.CommandText);

            using (Test.MutexLock.Begin())
            using (var sqlite = ApplicationDbScope.UseDefault())
            using (var trans = sqlite.BeginTransactionScope())
            {
                sqlite.OnExecuted += onExecuted;

                sqlite.Sql($"INSERT INTO main (CreationTime, Integer, Real, Text, Blob) VALUES ({DateTime.Now}, {416L}, {5.21d}, {"Hello"}, {"Hello".Bytes()});");

                sqlite.SqlQuery($"SELECT * FROM main WHERE Integer in {new[] { 415, 416, 417 }};").Then(records =>
                {
                    var record = records.First();
                    Assert.Equal(416L, record["Integer"]);
                    Assert.Equal(5.21d, record["Real"]);
                });
                sqlite.SqlQuery($"SELECT * FROM main WHERE Text={"Hello"};").Then(records =>
                {
                    var record = records.First();
                    Assert.Equal(416L, record["Integer"]);
                    Assert.Equal(5.21d, record["Real"]);
                });
                sqlite.SqlQuery($"SELECT * FROM main WHERE Blob={"Hello".Bytes()};").Then(records =>
                {
                    var record = records.First();
                    Assert.Equal(416L, record["Integer"]);
                    Assert.Equal(5.21d, record["Real"]);
                });

                sqlite.SqlQuery<Main>($"SELECT * FROM main WHERE Text={"Hello"};").Then(records =>
                {
                    var record = records.First();
                    Assert.Equal(5.21d, record.Real);
                });
                sqlite.Sql($"DELETE FROM main;");

                Assert.Equal(new[]
                {
                    "INSERT INTO main (CreationTime, Integer, Real, Text, Blob) VALUES (@p0, @p1, @p2, @p3, @p4);",
                    "SELECT * FROM main WHERE Integer in (@p0_0, @p0_1, @p0_2);",
                    "SELECT * FROM main WHERE Text=@p0;",
                    "SELECT * FROM main WHERE Blob=@p0;",
                    "SELECT * FROM main WHERE Text=@p0;",
                    "DELETE FROM main;",
                }, sqlList);
            }
        }

        [Fact]
        public void InjectionTest()
        {
            var sqlList = new List<string>();
            void onExecuted(SqliteCommand command) => sqlList.Add(command.CommandText);

            using (Test.MutexLock.Begin())
            using (var sqlite = ApplicationDbScope.UseDefault())
            {
                sqlite.OnExecuted += onExecuted;

                sqlite.Sql($"INSERT INTO main (CreationTime, Integer, Real, Text) VALUES ({DateTime.Now}, {416L}, {5.21d}, {"Hello"});");
                // SQL injection (concat - success)
                {
                    var text = "' or 1 or '";
                    var count = sqlite.UnsafeSqlQuery("SELECT * FROM main WHERE Text='" + text + "';").Length;
                    Assert.True(count > 0);
                }
                // SQL injection (failed)
                {
                    var text = "' or 1 or '";
                    var count = sqlite.SqlQuery($"SELECT * FROM main WHERE Text={text};").Length;
                    Assert.True(count == 0);
                }

                sqlite.OnExecuted -= onExecuted;
                sqlite.UnsafeSql($"DELETE FROM main;");

                Assert.Equal(new[]
                {
                    "INSERT INTO main (CreationTime, Integer, Real, Text) VALUES (@p0, @p1, @p2, @p3);",
                    "SELECT * FROM main WHERE Text='' or 1 or '';",
                    "SELECT * FROM main WHERE Text=@p0;",
                }, sqlList);
            }
        }

    }
}
