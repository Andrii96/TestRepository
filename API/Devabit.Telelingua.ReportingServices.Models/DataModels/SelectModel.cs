using System.Collections.Generic;

namespace Devabit.Telelingua.ReportingServices.Models.DataModels
{
    public class SelectModel
    {
        public string TableName { get; set; }

        public long TableId { get; set; }

        public string TableAlias { get; set; }

        public List<string> SelectedColumns { get; set; }

        public List<string> Joins { get; set; }

        public List<string> OrderBys { get; set; }

        public List<string> GroupBys { get; set; }

        public List<string> Functions { get; set; }
    }
}
