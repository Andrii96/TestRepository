using System;
using System.Collections.Generic;
using System.Text;

namespace Devabit.Telelingua.ReportingServices.Models.DataModels
{
    public class SavedScriptProcessModel
    {
        public long Id { get; set; }

        public List<SqlParameterModel> Parameters { get; set; }

        public PaginationModel Pagination { get; set; }

        public string ConnectionStringName { get; set; }
    }
}
