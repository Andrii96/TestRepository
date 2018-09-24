using System;
using System.Collections.Generic;
using System.Text;

namespace Devabit.Telelingua.ReportingServices.DataAccess.Models
{
    public class SqlScript
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public string SqlScriptText { get; set; }

        public string Parameters { get; set; }

        public long? CategoryId { get; set; }

        public Category Category { get; set; }
    }
}
