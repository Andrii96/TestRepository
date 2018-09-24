using System;
using System.Collections.Generic;
using System.Text;

namespace Devabit.Telelingua.ReportingServices.Models.DataModels
{
    public class SqlScriptModel
    {
        public SqlRequestModel SqlRequest { get; set; }
        public List<SqlParameterModel> Parameters { get; set; }
    }
}
