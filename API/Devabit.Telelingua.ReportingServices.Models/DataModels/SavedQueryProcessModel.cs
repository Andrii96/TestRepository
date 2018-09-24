using System.Collections.Generic;

namespace Devabit.Telelingua.ReportingServices.Models.DataModels
{
    public class SavedQueryProcessModel
    {
        public long Id { get; set; }

        public List<ExplicitFilterModel> Filters { get; set; }

        public List<ComparableFieldModel> Comparisons { get; set; }

        public List<CalculatedColumnModel> ExternalCalculations { get; set; }

        public string ConnectionStringName { get; set; }

        public PaginationModel Pagination { get; set; }
    }
}
