using Devabit.Telelingua.ReportingServices.Enums;

namespace Devabit.Telelingua.ReportingServices.Models.DataModels
{
    public class JoinModel
    {
        public TableViewModel ToTable { get; set; }


        public JoinTypes JoinType { get; set; } = JoinTypes.Left;

        public WhereGroup JoinCondition { get; set; }
    }
}