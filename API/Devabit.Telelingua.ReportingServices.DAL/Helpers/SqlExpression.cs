using System;
using System.Collections.Generic;
using System.Text;

namespace Devabit.Telelingua.ReportingServices.DAL.Helpers
{
    public class SqlExpression
    {
        public const string GetTableNameExpression = "select TABLE_SCHEMA, TABLE_NAME from INFORMATION_SCHEMA.TABLES;";
        public const string GetTableInfoExpression = "select COLUMN_NAME, DATA_TYPE from INFORMATION_SCHEMA.COLUMNS where TABLE_NAME=@p1 and TABLE_SCHEMA=@p2;";
        public static string CountExpression(string command) => $"select distinct COUNT(*) over() as count from ({command}) subb";

    }
}
