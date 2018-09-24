namespace Devabit.Telelingua.ReportingServices.Models.DataModels
{
    public class ReplaceIfCalculationModel
    {
        public string Comparison { get; set; }

        public string CompareTo { get; set; }

        public CalculatedColumnEntity ReplaceWith { get; set; }
    }
}
