using System;
using System.Collections.Generic;
using System.Text;

namespace Devabit.Telelingua.ReportingServices.Models.DataModels
{
    public class RowModel
    {
        public List<string> Values { get; set; }

        public QueryResultModel Internal { get; set; }
    }
}
