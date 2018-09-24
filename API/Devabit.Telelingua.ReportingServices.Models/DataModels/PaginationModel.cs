using System;
using System.Collections.Generic;
using System.Text;

namespace Devabit.Telelingua.ReportingServices.Models.DataModels
{
    public class PaginationModel
    {
        public long PageNumber { get; set; }

        public long PageSize { get; set; }

        public long Skip = 0;
    }
}
