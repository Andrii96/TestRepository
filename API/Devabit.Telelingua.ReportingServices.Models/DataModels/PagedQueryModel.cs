using System;
using System.Collections.Generic;
using System.Text;

namespace Devabit.Telelingua.ReportingServices.Models.DataModels
{
    public class PagedQueryModel
    {
        public List<QueryModel> Queries { get; set; }

        public List<ColorRuleModel> ColorRules { get; set; }

        public PaginationModel PagingModel { get; set; }

        public string ConnectionStringName { get; set; }
    }
}
