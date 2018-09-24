using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Devabit.Telelingua.ReportingServices.Enums;

namespace Devabit.Telelingua.ReportingServices.Helpers
{
    public static class EnumConverter
    {
        public static ColumnTypes ConvertToColumnType(string type)
        {
            return new[] { "char", "varchar", "text", "nchar", "nvarchar", "ntext", "binary", "varbinary", "image" }
                .Contains(type)
                ? ColumnTypes.String
                : (new[]
                {
                    "tinyint", "smallint", "int", "bigint", "decimal", "numeric", "smallmoney", "money", "float", "real"
                }.Contains(type)
                    ? ColumnTypes.Number
                    : (new[]
                    {
                        "datetime", "datetime2", "smalldatetime", "date", "time", "datetimeoffset", "timestamp"
                    }.Contains(type)
                        ? ColumnTypes.Date
                        : ColumnTypes.Bool));
        }
    }
}
