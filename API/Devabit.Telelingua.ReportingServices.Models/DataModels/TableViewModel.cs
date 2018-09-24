using System.ComponentModel.DataAnnotations;

namespace Devabit.Telelingua.ReportingServices.Models.DataModels
{
    /// <summary>
    /// The model representing table in db.
    /// </summary>
    public class TableViewModel
    {
        /// <summary>
        /// Tables name.
        /// </summary>
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// Schema name.
        /// </summary>
        [Required]
        public string Schema { get; set; }

        /// <summary>
        /// The identifier of table to be joined.
        /// </summary>
        [Required]
        public long Id { get; set; }

        public override string ToString() => $"{Schema}.{Name}";
    }
}
