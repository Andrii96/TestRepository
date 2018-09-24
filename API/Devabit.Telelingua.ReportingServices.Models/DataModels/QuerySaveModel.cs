using System;
using System.Collections.Generic;
using System.Text;

namespace Devabit.Telelingua.ReportingServices.Models.DataModels
{
    public class QuerySaveModel
    {
        public string Name { get; set; }

        public PagedQueryModel Query { get; set; }

        public string SqlQuery { get; set; }

        public bool IsSqlChanged { get; set; }

        public long? CategoryId { get; set; }
    }
}
