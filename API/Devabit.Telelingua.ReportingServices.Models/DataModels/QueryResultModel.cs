using System.Collections.Generic;

namespace Devabit.Telelingua.ReportingServices.Models.DataModels
{
    public class QueryResultModel
    {
        public List<HeaderModel> ColumnHeaders { get; set; }

        public List<RowModel> Rows { get; set; }

    }
}
