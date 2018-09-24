using Devabit.Telelingua.ReportingServices.Models.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Devabit.Telelingua.ReportingServices.DAL.Helpers
{
    public static class ExtensionHelper
    {
        /// <summary>
        /// Gets the explicit filters for query.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>List of explicit filters.</returns>
        public static List<ExplicitFilterModel> GetExplicitFilters(this QueryModel queryModel)
        {
            return queryModel.TableQueries
                             .SelectMany(table => table.ExplicitFilters
                                                       .Select(filter => new ExplicitFilterModel
                                                       {
                                                           TableName = table.TableName,
                                                           SchemaName = table.TableSchema,
                                                           ColumnName = filter.ColumnName,
                                                       })).ToList();
        }

        /// <summary>
        /// Gets the columns for query.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>List of query columns.</returns>
        public static List<ColumnModel> GetColumnsList(this QueryModel query)
        {
            return query.TableQueries
                        .SelectMany(x => (x.SelectedColumns.Select(y => $"{y.Alias}").Concat(x.GroupByColumns)
                                                          .Concat(x.Functions.Select(func => func.Alias)))
                                                          .Select(y => new ColumnModel()
                                                          {
                                                              TableName = x.TableName,
                                                              ColumnAlias = y
                                                          }))
                        .Concat(query.CalculatedColumns.Select(x => new ColumnModel()
                        {
                            TableName = "CalculatedCollumn",
                            ColumnAlias = x.Alias
                        })).ToList();
        }

        /// <summary>
        /// Creates base select statement (select [distinct] [columns] from [table] left join [table1] on [expression] where [expression] group by [columns])
        /// </summary>
        /// <param name="selectModels">list of select models</param>
        /// <param name="distinct">specifies distinct select</param>
        /// <returns>SqlQuery object</returns>
        public static SqlQuery CreateBaseSqlSelectCommand(this List<SelectModel> selectModels, bool distinct)
        {
            var query = new SqlQuery();
            query = distinct ? query.AddDistinct() : query;
            return query.AddSelectedColumns(selectModels.SelectMany(selectedModel => selectedModel.SelectedColumns).ToList())
                        .AddFunctions(selectModels.SelectMany(selectModel => selectModel.Functions).ToList())
                        .AddSelectionTable(selectModels.Select(selectModel => $"{selectModel.TableName} {selectModel.TableAlias}").First())
                        .AddJoins(selectModels.SelectMany(selectModel => selectModel.Joins).ToList())
                        .AddGroupBys(selectModels.SelectMany(selectModel => selectModel.GroupBys).ToList());
        }

        public static void ExtendSqlQueryWithFilters(this SqlQuery query, List<WhereGroup> filters, List<SelectModel> tables, bool processExternals = true)
        {
            var allWheres = new List<string>();
            foreach (var filter in filters)
            {
                allWheres.AddRange(ParseWhereGroup(filter, tables, processExternals));
            }

            var lastContainingWhere = allWheres.LastOrDefault();
            if (lastContainingWhere != null)
            {
                var resultString = lastContainingWhere.Remove(lastContainingWhere.LastIndexOf(')') + 1);
                allWheres[allWheres.Count - 1] = resultString;
            }

            query.AddWheres(allWheres);
        }


        public static void ExtendSqlQueryWithOrderBy(this SqlQuery query, List<SelectModel> selectModels)
        {
            var orderByColumns = selectModels.Any(selectModel => selectModel.OrderBys.Any()) ?
                                      selectModels.SelectMany(selectModel => selectModel.OrderBys).ToList() :
                                      new List<string> { new Func<string>(() =>  selectModels.All(sm => sm.SelectedColumns.Count == 0) ?
                                                                                selectModels.First(x => x.Functions.Any()).Functions.First().Split(" as ")[1] :
                                                                                selectModels.First(x => x.SelectedColumns.Any()).SelectedColumns.First().Split(" as ")[1])()  };
            if (query.HasGroupBy)
            {
                orderByColumns = query.GroupByColumns;
            }

            query.AddOrderBys(orderByColumns);
        }
        /// <summary>
        /// Changes SqlQuery object by adding order by, offset and fetch next statements.
        /// </summary>
        /// <param name="query">Query to be changed</param>
        /// <param name="selectModels">list of select models</param>
        /// <param name="offset"></param>
        /// <param name="count"> page size</param>
        public static void ExtendSqlQueryWithPagination(this SqlQuery query, List<SelectModel> selectModels, long offset, long count)
        {
                  query.ExtendSqlQueryWithOrderBy(selectModels);
                  query.AddOffset(offset).AddFetch(count);
        }

        /// <summary>
        /// Sets internal rows to query result
        /// </summary>
        /// <param name="resultModel"></param>
        /// <param name="internalModel"></param>
        public static void SetInternalRows(this QueryResultModel resultModel, QueryResultModel internalModel, List<string> groupBys)
        {
            foreach (var row in resultModel.Rows)
            {
                var internalRows = internalModel.Rows.Where(internalRow =>
                {
                    for (int columnNumber = 0; columnNumber < row.Values.Count; columnNumber++)
                    {
                        if (!groupBys.Contains(resultModel.ColumnHeaders[columnNumber].Name))
                        {
                            continue;
                        }

                        if (row.Values[columnNumber] != internalRow.Values[columnNumber])
                        {
                            return false;
                        }
                    }
                    return true;
                }).ToList();

                row.Internal = new QueryResultModel
                {
                    ColumnHeaders = internalModel.ColumnHeaders,
                    Rows = internalRows
                };
            }
        }

        public static void PrepareCalculations(this QueryModel model)
        {
            foreach (var calculation in model.CalculatedColumns)
            {
                ChangeCalculationAliases(model, calculation.Calculation);
            }
        }

        public static void ExtendResultWithComparisons(this QueryResultModel resultModel, QueryResultModel comparisonsResult, List<string> groupColumns, int columnIndex)
        {
            if (columnIndex == -1) return;
            var groupColumnsIndexes = groupColumns.Select(g => comparisonsResult
                                                     .ColumnHeaders
                                                     .FindIndex(header => header.Name == g));
            foreach (var row in resultModel.Rows)
            {
                row.Values.Insert(columnIndex + 1, string.Empty);
                comparisonsResult.Rows.ForEach(comparisonRow =>
                {
                    if (groupColumnsIndexes.All(index => columnIndex != -1 && row.Values[index] == comparisonRow.Values[index]))
                    {
                        row.Values[columnIndex + 1] = comparisonRow.Values[columnIndex];
                        return;
                    }
                });
            }
        }

        private static void ChangeCalculationAliases(QueryModel model, CalculatedColumnEntity calculation)
        {
            if (calculation.EntityType == Enums.CalculatedEntityType.Column ||
                calculation.EntityType == Enums.CalculatedEntityType.Function)
            {
                calculation.Value = model.TableQueries.SelectMany(x => x.SelectedColumns).FirstOrDefault(x => x.ColumnName == calculation.Value).Alias;
            }
            if (calculation.LeftOp != null)
            {
                ChangeCalculationAliases(model, calculation.LeftOp);
            }
            if (calculation.RightOp != null)
            {
                ChangeCalculationAliases(model, calculation.RightOp);
            }
        }

        private static List<string> ParseWhereGroup(WhereGroup whereGroup, List<SelectModel> tables, bool processExternals)
        {
            if (!processExternals && whereGroup.IsExternal)
            {
                return new List<string>();
            }
            var parsedWheres = new List<string>();
            var statement = "(";
            foreach (var whereStatement in whereGroup.WhereStatements)
            {
                if (parsedWheres.Count > 0)
                {
                    statement = string.Empty;
                }
                if (!string.IsNullOrEmpty(whereStatement.Value))
                {
                    ChangeComparison(whereStatement);
                }
                
                var tableAlias = tables.FirstOrDefault(x => x.TableId == whereStatement.TableId).TableAlias;
                

                statement += UseAlias(whereStatement, tableAlias) ?
                    $"{tableAlias}.{ whereStatement.ColumnAlias} " :
                    $"{whereStatement.ColumnAlias}";

                if (whereStatement.Value==null)
                {
                    var compareTableAlias = tables.FirstOrDefault(t => t.TableId == whereStatement.CompareWithColumn.TableId).TableAlias;
                    statement+= $"{whereStatement.Comparison} {compareTableAlias }.{whereStatement.CompareWithColumn.ColumnAlias}";
                }
                else
                {
                    statement += $"{whereStatement.Comparison} {whereStatement.Value}";
                }
                statement += whereStatement.UseOr ? " or " : " and ";
                parsedWheres.Add(statement);
            }
            if (parsedWheres.Count == 0 && whereGroup.WhereGroups.Count>0)
            {
                parsedWheres.Add(statement);
            }
            parsedWheres.AddRange(whereGroup.WhereGroups.SelectMany(where => ParseWhereGroup(where, tables, processExternals)));

            var last = parsedWheres.LastOrDefault();
            if (last != null)
            {
                var lastOr = last.LastIndexOf("or");
                var lastAnd = last.LastIndexOf("and");
                if (lastAnd != -1 || lastOr != -1)
                {
                    last = last.Insert(lastOr > lastAnd ? lastOr : lastAnd, ")");
                    parsedWheres[parsedWheres.Count - 1] = last;
                }
            }
            
            return parsedWheres;
        }

        private static void ChangeComparison(WhereModel whereModel)
        {
            if (@whereModel.Value.ToLower() == "null")
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
                if (!whereModel.Value.Contains("'") && whereModel.Value.ToLower() != "null")
                    @whereModel.Value = $"\'{@whereModel.Value.Replace("'", "''")}\'";
            }
        }

        private static bool UseAlias(WhereModel where, string tableAlias)
        {
            var match = Regex.Match(where.ColumnAlias, @"year\((.*)\)");

            if (match.Success)
            {
                where.ColumnAlias = where.ColumnAlias.Replace(match.Groups[1].Value, tableAlias + "." + match.Groups[1].Value);
                return false;
            }
            return true;
        }

        //private static bool HasSqlScriptOrderBy(this string sqlStatement)
        //{
        //    var regex = new Regex(@"order\s+by", RegexOptions.IgnoreCase);
        //    return regex.IsMatch(sqlStatement);
        //}

        //private static string GetFirstSelectedColumn(this string sqlStatement)
        //{
        //    var regex = new Regex(@"select\s+(.*?)(?=\s|,)", RegexOptions.IgnoreCase);
        //    var matchings = regex.Match(sqlStatement);
        //    while (matchings.NextMatch().Success)
        //    {
        //        matchings = matchings.NextMatch();
        //    }
        //    return matchings.Groups[1].Value;
        //}

        //public static void AddPaginationToScript(this string sqlStatement, long offset, long pageSize)
        //{
        //    var offsetString = $" OFFSET {offset} ROWS";
        //    var fetchString = $" FETCH NEXT {pageSize} ROWS ONLY";
        //    if (!sqlStatement.HasSqlScriptOrderBy())
        //    {
        //        sqlStatement += $" ORDER BY {sqlStatement.GetFirstSelectedColumn()} ";
        //    }

        //    sqlStatement += offsetString;
        //    sqlStatement += fetchString;
        //}
    }
}
