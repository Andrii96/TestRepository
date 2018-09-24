using System;
using System.Collections.Generic;
using System.Text;

namespace Devabit.Telelingua.ReportingServices.DataAccess.Models
{
    public class Category
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public List<Query> Queries { get; set; }

        public List<SqlScript> SqlScripts {get;set;}
    }
}
