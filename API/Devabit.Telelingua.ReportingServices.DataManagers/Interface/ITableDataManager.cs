using System;
using System.Collections.Generic;
using System.Text;
using Devabit.Telelingua.ReportingServices.Models.DataModels;

namespace Devabit.Telelingua.ReportingServices.DataManagers.Interface
{
    public interface ITableDataManager
    {
        /// <summary>
        /// Gets the list of tables names.
        /// </summary>
        /// <returns></returns>
        IEnumerable<TableViewModel> GetTableNames();

        /// <summary>
        /// Gets info about specified table
        /// </summary>
        /// <param name="tableName">Table model representing table`s name</param>
        /// <returns>Corresponding table view model.</returns>
        TableDataModel GetTable(TableViewModel tableName);

        /// <summary>
        /// Processes the query.
        /// </summary>
        /// <param name="query">The query to be processed.</param>
        /// <returns>The query result.</returns>
        PagedQueryResultModel ProcessQuery(PagedQueryModel queries, List<ComparableFieldModel> comparisons = null,
            List<CalculatedColumnModel> externalCalculation = null);

        /// <summary>
        /// Process sql script
        /// </summary>
        /// <param name="sqlScriptModel">The query to be processed</param>
        /// <returns>The query result</returns>
        PagedQueryResultModel ProcessSqlScript(SqlScriptModel sqlScriptModel);
    }
}
