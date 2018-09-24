namespace Devabit.Telelingua.ReportingServices.DataAccess.Models
{
    public class Query
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public string QueryText { get; set; }

        public string SqlQuery { get; set; }

        public bool IsSqlEdited { get; set; }

        public long? CategoryId { get; set; }

        public Category Category { get; set; }
    }
}
