using System.Collections.Generic;

namespace Devabit.Telelingua.ReportingServices.Models.DataModels
{
    public class QueryTableModel
    {
        public long Id { get; set; }

        public string TableName { get; set; }

        public string TableSchema { get; set; }

        public List<SelectedColumnModel> SelectedColumns { get; set; }

        public List<JoinModel> Joins { get; set; }

        public List<SelectedColumnModel> ExplicitFilters { get; set; }

        public List<string> GroupByColumns { get; set; }

        public List<OrderByModel> Sortings { get; set; }

        public List<FunctionModel> Functions { get; set; }
    }
}
