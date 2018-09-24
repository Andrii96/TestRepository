using System;
using System.Collections.Generic;
using System.Text;

namespace Devabit.Telelingua.ReportingServices.Models.DataModels
{
    public class InnerQueryModel
    {
        public PagedQueryModel SavedQueryTemplate { get; set; }
        public SavedScriptModel SavedSqlScript { get; set; }
    }
}
