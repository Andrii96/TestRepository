using System;
using System.Collections.Generic;
using System.Text;

namespace Devabit.Telelingua.ReportingServices.Models.DataModels
{
    public class SavedQueryModel
    {
        public List<ExplicitFilterModel> Filters { get; set; }

        public List<ColumnModel> Columns { get; set; }
    }
}
