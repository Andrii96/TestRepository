using System.Collections.Generic;
using Devabit.Telelingua.ReportingServices.Enums;

namespace Devabit.Telelingua.ReportingServices.Models.DataModels
{
    public class TableDataModel
    {
        public long TableId { get; set; }
        public string Name { get; set; }

        public Dictionary<string,ColumnTypes> Columns { get; set; }
    }
}
