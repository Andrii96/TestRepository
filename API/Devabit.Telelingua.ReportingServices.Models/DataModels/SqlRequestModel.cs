using System;
using System.Collections.Generic;
using System.Text;

namespace Devabit.Telelingua.ReportingServices.Models.DataModels
{
    public class SqlRequestModel
    {
        public string SqlScript { get; set; }

        public PaginationModel PaginationModel { get; set; }

        public string ConnectionStringName { get; set; }
    }
}
