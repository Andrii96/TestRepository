using System.Collections.Generic;
using System.Linq;
using Devabit.Telelingua.ReportingServices.Models.DataModels;

namespace Devabit.Telelingua.ReportingServices.Calculation
{
    public class CalculationProcessor
    {
        /// <summary>
        /// Processes the calculations for model.
        /// </summary>
        /// <param name="results">The results model.</param>
        /// <param name="calculations">List of calculations to be processed.</param>
        public static void ProcessCalculatedColumns(PagedQueryResultModel results, List<CalculatedColumnModel> calculations)
        {
            if (calculations.Count() == 0)
            {
                return;
            }

            var groupWideCalculations = calculations.Where(x => x.Calculation.IsGroupWide(results.Result.ColumnHeaders)).ToList();
            var internalCalculations = calculations.Except(groupWideCalculations).ToList();
            foreach (var calculation in internalCalculations)
            {
                if (results.Result.Rows[0].Internal == null)
                {
                    groupWideCalculations.Add(calculation);
                    continue;
                }
                var type = "calculated";
                foreach (var row in results.Result.Rows)
                {
                    foreach (var internalRow in row.Internal.Rows)
                    {
                        var result = CalculationParser.ProcessCalculation(calculation.Calculation, row.Internal.ColumnHeaders,
                            internalRow);
                        type = result.GetResultType();
                        internalRow.Values.Add(result.ToString());
                    }
                }
                results.Result.Rows[0].Internal.ColumnHeaders.Add(new HeaderModel { Name = calculation.Alias, Type = type });

            }

            foreach (var calculation in groupWideCalculations)
            {
                var type = "calculated";
                foreach (var row in results.Result.Rows)
                {
                    var result = CalculationParser.ProcessCalculation(calculation.Calculation, results.Result.ColumnHeaders, row);
                    type = result.GetResultType();
                    row.Values.Add(result.ToString());
                }
                results.Result.ColumnHeaders.Add(new HeaderModel { Name = calculation.Alias, Type = type });
            }
        }
    }
}
