using System;
using System.Collections.Generic;
using System.Text;

namespace Devabit.Telelingua.ReportingServices.Models.DataModels
{
    public class PagedQueryResultModel
    {
        public long PageNumber { get; set; }

        public long PageCount { get; set; }

        public int TotalCount { get; set; }

        public bool HasNext { get; set; }

        public QueryResultModel Result { get; set; }
    }
}
