namespace Devabit.Telelingua.ReportingServices.Models.DataModels
{
    /// <summary>
    /// Base model for other comparison models.
    /// </summary>
    public class ComparisonModel
    {
        public long TableId { get; set; }

        public string ColumnAlias { get; set; }

        public string Comparison { get; set; } = "=";

        public ColumnModel CompareWithColumn { get; set; }

        public string Value { get; set; }
    }
}
