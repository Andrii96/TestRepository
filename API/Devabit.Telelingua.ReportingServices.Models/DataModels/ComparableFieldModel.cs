using System;
using System.Collections.Generic;
using System.Text;

namespace Devabit.Telelingua.ReportingServices.Models.DataModels
{
    public class ComparableFieldModel
    {
        public ColorRuleModel Column { get; set; }

        public string Alias { get; set; }

        public List<ExplicitFilterModel> Filters { get; set; }
    }
}
