using System;
using System.Collections.Generic;
using System.Text;

namespace Devabit.Telelingua.ReportingServices.Models.DataModels
{
    public class SqlScriptSaveModel
    {
        public string Name { get; set; }

        public List<string> Parameters { get; set; }
        
        public  SqlRequestModel SqlRequest { get; set; }

        public long? CategoryId { get; set; }
    }
}
