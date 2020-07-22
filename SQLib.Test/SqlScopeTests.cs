using Microsoft.Data.Sqlite;
using NStandard;
using SQLib.Data.Test;
using SQLib.Test.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace SQLib.Test
{
    public class SqlScopeTests
    {
        [Fact]
        public void Test1()
        {
            using (Test.MutexLock.Begin())
            using (var sqlite = ApplicationDbScope.UseDefault())
            {
                var creationTime = DateTime.Now;
                var output = new StringBuilder();

                sqlite.OnExecuted += command => output.AppendLine(command.CommandText);
                sqlite.Sql($"INSERT INTO main (CreationTime, Integer, Real, Text) VALUES ({creationTime}, {416L}, {5.21d}, {"Hello"});");

                var text = "Hello";
                sqlite.SqlQuery($"SELECT * FROM main WHERE Text={text};").First().Then(record =>
                {
                    Assert.Equal(416L, record["Integer"]);
                    Assert.Equal(5.21d, record["Real"]);
                });
                var mains = sqlite.SqlQuery<Main>($"SELECT * FROM main WHERE Text={"Hello"};").First().Then(record =>
                {
                    Assert.Equal(5.21d, record.Real);
                });

                sqlite.Sql($"DELETE FROM main;");

                Assert.Equal(@"INSERT INTO main (CreationTime, Integer, Real, Text) VALUES (@p0, @p1, @p2, @p3);
SELECT * FROM main WHERE Text=@p0;
SELECT * FROM main WHERE Text=@p0;
DELETE FROM main;
", output.ToString());
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
                var creationTime = DateTime.Now;

                sqlite.Sql($"INSERT INTO main (CreationTime, Integer, Real, Text) VALUES ({creationTime}, {416L}, {5.21d}, {"Hello"});");
                sqlite.OnExecuted += onExecuted;

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
                    "SELECT * FROM main WHERE Text='' or 1 or '';",
                    "SELECT * FROM main WHERE Text=@p0;",
                }, sqlList);
            }
        }

    }
}
