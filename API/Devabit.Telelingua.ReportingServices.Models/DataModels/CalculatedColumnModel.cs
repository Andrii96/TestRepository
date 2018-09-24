using System;
using System.Collections.Generic;
using System.Text;

namespace Devabit.Telelingua.ReportingServices.Models.DataModels
{
    public class CalculatedColumnModel
    {
        public string Alias { get; set; }

        public CalculatedColumnEntity Calculation { get; set; }
    }
}
