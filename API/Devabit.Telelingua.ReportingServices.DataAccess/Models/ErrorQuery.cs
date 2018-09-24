using System;

namespace Devabit.Telelingua.ReportingServices.DataAccess.Models
{
    public class ErrorQuery
    {
        public long Id { get; set; }

        public string QueryText { get; set; }

        public DateTime Date { get; set; }

        public string Exception { get; set; }
    }
}
