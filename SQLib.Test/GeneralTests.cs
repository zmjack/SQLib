using Microsoft.Data.Sqlite;
using NStandard;
using SQLib.Data.Test;
using System;
using System.Collections.Generic;
using Xunit;

namespace SQLib.Test
{
    public class GeneralTests
    {
        [Fact]
        public void Test1()
        {
            var sqlList = new List<string>();
            void onExecuted(SqliteCommand command) => sqlList.Add(command.CommandText);

            using (Test.MutexLock.Begin())
            using (var sqlite = ApplicationDbScope.UseDefault())
            {
                sqlite.OnExecuted += onExecuted;

                sqlite.Sql($"INSERT INTO bin (CreationTime, Binary) VALUES ({DateTime.Now}, {"Hello".Bytes()});");
                sqlite.Sql($"DELETE FROM bin;");

                Assert.Equal(new[]
                {
                    "INSERT INTO bin (CreationTime, Binary) VALUES (@p0, @p1);",
                    "DELETE FROM bin;",
                }, sqlList);
            }
        }

    }
}
