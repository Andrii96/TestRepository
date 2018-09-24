using System.Collections.Generic;

namespace Devabit.Telelingua.ReportingServices.Models.DataModels
{
    public class QueryModel
    {
        public List<QueryTableModel> TableQueries { get; set; }

        public bool SelectDistinct { get; set; }

        public bool ShowDetails { get; set; }

        public List<CalculatedColumnModel> CalculatedColumns { get; set; }

        public List<WhereGroup> Filters { get; set; }

        public object TablePositionData { get; set; }
    }
}
