using System;
using System.Collections.Generic;
using System.Text;
using Devabit.Telelingua.ReportingServices.Enums;

namespace Devabit.Telelingua.ReportingServices.Models.DataModels
{
    public class ExplicitFilterModel
    {
        public string TableName { get; set; }

        public string SchemaName { get; set; }

        public string ColumnName { get; set; }

        public ColumnTypes ColumnType { get; set; }

        public string Value { get; set; } = "";

        public string ComparisonType { get; set; } = "";
        public bool UseOr { get; set; } = false;
    }
}
