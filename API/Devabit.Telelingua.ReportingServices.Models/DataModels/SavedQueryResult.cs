using System;
using System.Collections.Generic;
using System.Text;

namespace Devabit.Telelingua.ReportingServices.Models.DataModels
{
    public class SavedQueryResult
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public PagedQueryModel Query { get; set; }

        public PagedQueryResultModel QueryResult { get; set; }
    }
}
