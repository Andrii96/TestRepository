using Devabit.Telelingua.ReportingServices.Models.DataModels;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Devabit.Telelingua.ReportingServices.DAL.Helpers
{
    public class QueryConverter
    {
        #region Fields
        /// <summary>
        /// Dictionary for saving table alias. Key - table id, Value - table alias.
        /// </summary>
        private Dictionary<long, string> TablesAlias { get; set; } = new Dictionary<long, string>();

        /// <summary>
        /// List of tables info
        /// </summary>
        private List<TableDataModel> Tables { get; set; }
        #endregion

        #region Constructor
        public QueryConverter(List<TableDataModel> tables)
        {
            Tables = tables;
            int counter = 0;
            tables.ForEach(table => TablesAlias.Add(table.TableId, GetTableAlias(counter++)));
        }
        #endregion

        #region Methods

        /// <summary>
        /// Converts to query to SelectModel
        /// </summary>
        /// <param name="query">query to be converted</param>
        /// <returns>Converted to SelectModel query</returns>
        public SelectModel ToSelectModel(QueryTableModel query, bool processExternals = true)
        {

            var tableAlias = TablesAlias[query.Id];
            return new SelectModel
            {
                TableName = $"{query.TableSchema}.{query.TableName}",
                TableId = query.Id,
                TableAlias = tableAlias,
                SelectedColumns = query.SelectedColumns
                                       .Select(column => $"{tableAlias}.{column.ColumnName} as {column.Alias}")
                                       .ToList(),
                Functions = query.Functions
                                 .Select(function => $"{function.Function}({tableAlias}.{function.ColumnAlias}) as {function.Alias}")
                                 .ToList(),
                Joins = ParseJoins(query.Joins, query.Id),
                GroupBys = query.GroupByColumns
                                .Select(columnName => $"{tableAlias}.{columnName}")
                                .ToList(),
                OrderBys = query.Sortings
                                .Select(sorting => $"{sorting.OrderByAlias} {sorting.Direction}")
                                .ToList()

            };
        }
        #endregion

        #region Helpers


        private List<string> ParseJoins(List<JoinModel> joins, long fromTableId)
        {
            var joinStrings = joins.Select(join =>
                                            {
                                                var joinToAlias = string.Empty;
                                                if (join.ToTable.Id != fromTableId && TablesAlias.ContainsKey(join.ToTable.Id))
                                                {
                                                    joinToAlias = TablesAlias[join.ToTable.Id];
                                                }
                                                else
                                                {
                                                    joinToAlias = GetTableAlias(TablesAlias.Count + 1);
                                                    TablesAlias.Add(join.ToTable.Id, joinToAlias);
                                                }
                                                var joinCondition = ParseJoinCondition(join.JoinCondition,TablesAlias[fromTableId], joinToAlias);
                                                var lastJoinCondition = joinCondition.LastOrDefault();
                                                if (lastJoinCondition != null)
                                                {
                                                    var resultString = lastJoinCondition.Remove(lastJoinCondition.LastIndexOf(')') + 1);
                                                    joinCondition[joinCondition.Count - 1] = resultString;
                                                }
                                                return $"{ join.JoinType.ToString()} join {join.ToTable.ToString()} {joinToAlias} " +
                                                       $"on {string.Join(" ",joinCondition )} ";
                                            });
            return joinStrings.ToList();
        }


        private List<string> ParseJoinCondition(WhereGroup condition,string fromAlias, string toAlias)
        {
            var parsedWheres = new List<string>();
            var statement = "(";
            foreach (var whereStatement in condition.WhereStatements)
            {
                if (parsedWheres.Count > 0)
                {
                    statement = string.Empty;
                }
                
                statement += UseAlias(whereStatement, fromAlias) ?
                                 $"{fromAlias}.{ whereStatement.ColumnAlias} " :
                                 $"{whereStatement.ColumnAlias}";
                statement += $"{whereStatement.Comparison} {toAlias}.{whereStatement.CompareWithColumn.ColumnAlias}";
                statement += whereStatement.UseOr ? " or " : " and ";
                parsedWheres.Add(statement);
            }

            if (parsedWheres.Count == 0 && condition.WhereGroups.Count > 0)
            {
                parsedWheres.Add(statement);
            }
            parsedWheres.AddRange(condition.WhereGroups.SelectMany(where => ParseJoinCondition(where,fromAlias, toAlias)));

            var last = parsedWheres.LastOrDefault();
            if (last != null)
            {
                var lastOr = last.LastIndexOf("or");
                var lastAnd = last.LastIndexOf("and");
                if (lastOr != -1 || lastAnd != -1)
                {
                    last = last.Insert(lastOr > lastAnd ? lastOr : lastAnd, ")");
                    
                    parsedWheres[parsedWheres.Count - 1] = last;
                }
            }
            
            return parsedWheres;
        }

        private void ChangeComparison(WhereModel whereModel)
        {
            if (@whereModel.Value == "null")
            {
                switch (@whereModel.Comparison)
                {
                    case "=":
                        @whereModel.Comparison = " is ";
                        break;
                    case "<>":
                        @whereModel.Comparison = " is not ";
                        break;
                }
            }
            else
            {
                if (!whereModel.Value.Contains("'"))
                    @whereModel.Value = $"\'{@whereModel.Value.Replace("'", "''")}\'";
            }
        }

        private bool UseAlias(WhereModel where, string tableAlias)
        {
            var match = Regex.Match(where.ColumnAlias, @"year\((.*)\)");

            if (match.Success)
            {
                where.ColumnAlias = where.ColumnAlias.Replace(match.Groups[1].Value, tableAlias + "." + match.Groups[1].Value);
                return false;
            }
            return true;
        }

        private string GetTableAlias(int number) => $"T{number}";

        private void ChangeDateTypeColumnsName(WhereGroup filter, TableDataModel tableInfo)
        {
            if (filter == null)
            {
                return;
            }
            foreach (var where in filter.WhereStatements)
            {
                if (tableInfo.Columns.ContainsKey(where.ColumnAlias) && tableInfo.Columns[where.ColumnAlias] == Enums.ColumnTypes.Date && where.Value.Length == 4)
                {
                    where.ColumnAlias = $"year({where.ColumnAlias})";
                }
            }
            filter.WhereGroups.ForEach(x => ChangeDateTypeColumnsName(x, tableInfo));
        }
        #endregion

    }
}
