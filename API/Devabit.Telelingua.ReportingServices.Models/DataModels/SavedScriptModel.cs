using System;
using System.Collections.Generic;
using System.Text;

namespace Devabit.Telelingua.ReportingServices.Models.DataModels
{
    public class SavedScriptModel
    {
        public string SqlScript { get; set; }

        public List<string> Parameters { get; set; }

        public string ConnectionStringName { get; set; }
    }
}
