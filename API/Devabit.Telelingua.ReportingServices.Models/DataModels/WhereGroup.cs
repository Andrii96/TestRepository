using System.Collections.Generic;

namespace Devabit.Telelingua.ReportingServices.Models.DataModels
{
    public class WhereGroup
    {
        public List<WhereModel> WhereStatements { get; set; }

        public List<WhereGroup> WhereGroups { get; set; }

        public bool IsExternal { get; set; } = false;
    }
}