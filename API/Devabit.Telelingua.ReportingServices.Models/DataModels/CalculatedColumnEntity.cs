using System.Collections.Generic;
using Devabit.Telelingua.ReportingServices.Enums;

namespace Devabit.Telelingua.ReportingServices.Models.DataModels
{
    public class CalculatedColumnEntity
    {
        public string Action { get; set; }

        public CalculatedColumnEntity LeftOp { get; set; }

        public CalculatedColumnEntity RightOp { get; set; }

        public string Value { get; set; }

        public CalculatedEntityType EntityType { get; set; }

        public List<ReplaceIfCalculationModel> ReplaceIfOptions { get; set; }
    }
}