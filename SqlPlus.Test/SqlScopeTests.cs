using NStandard;
using SqlPlus.Data.Test;
using System;
using System.Linq;
using System.Text;
using Xunit;

namespace SqlPlus.Test
{
    public class SqlScopeTests
    {
        private static readonly Lock SqlScopeTestsLock = new Lock();

        [Fact]
        public void Test1()
        {
            using (SqlScopeTestsLock.Begin())
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

                sqlite.Sql($"DELETE FROM main;");

                Assert.Equal(@"INSERT INTO main (CreationTime, Integer, Real, Text) VALUES (@p0, @p1, @p2, @p3);
SELECT * FROM main WHERE Text=@p0;
DELETE FROM main;
", output.ToString());
            }
        }

        [Fact]
        public void InjectionTest()
        {
            using (SqlScopeTestsLock.Begin())
            using (var sqlite = ApplicationDbScope.UseDefault())
            {
                var creationTime = DateTime.Now;
                var output = new StringBuilder();

                sqlite.OnExecuted += command => output.AppendLine(command.CommandText);
                sqlite.Sql($"INSERT INTO main (CreationTime, Integer, Real, Text) VALUES ({creationTime}, {416L}, {5.21d}, {"Hello"});");

                var text = "' or 1 or '";
                var count = sqlite.UnsafeSqlQuery("SELECT * FROM main WHERE Text='" + text + "';").Length;
                Assert.True(count > 0);

                sqlite.UnsafeSql($"DELETE FROM main;");

                Assert.Equal(@"INSERT INTO main (CreationTime, Integer, Real, Text) VALUES (@p0, @p1, @p2, @p3);
SELECT * FROM main WHERE Text='' or 1 or '';
DELETE FROM main;
", output.ToString());
            }
        }

    }
}
