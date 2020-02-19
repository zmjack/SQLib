using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Northwnd;
using NStandard;
using SqlPlusApp.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace SqlPlusApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var regions = new[]
            {
                QueryRegion_Traditional(1),
                QueryRegion_SqlPlus(1),
                QueryRegion_EF(1),
            };
            regions.Dump();

            Console.WriteLine(regions.All(x => x == "Eastern"));
        }

        static string QueryRegion_Traditional(int regionId)
        {
            string ret;

            var conn = new SqliteConnection("filename=northwnd.db");
            conn.Open();

            var cmd = new SqliteCommand("SELECT RegionDescription FROM Regions WHERE RegionId=@p0;", conn);
            cmd.Parameters.Add(new SqliteParameter
            {
                ParameterName = "@p0",
                Value = regionId,
                DbType = DbType.Int32,
            });

            var reader = cmd.ExecuteReader();
            reader.Read();
            ret = reader.GetString(0);
            reader.Close();

            conn.Close();

            return ret;
        }

        static string QueryRegion_SqlPlus(int regionId)
        {
            using (var sqlite = new ApplicationDbScope(new SqliteConnection("filename=northwnd.db")))
            {
                var region = sqlite.SqlQuery($"SELECT RegionDescription FROM Regions WHERE RegionId={regionId};").First();
                return region["RegionDescription"] as string;
            }
        }

        static string QueryRegion_EF(int regionId)
        {
            var options = new DbContextOptionsBuilder().UseSqlite("filename=northwnd.db").Options;
            using (var context = new NorthwndContext(options))
            {
                return context.Regions.First(x => x.RegionID == regionId).RegionDescription;
            }
        }

    }
}
