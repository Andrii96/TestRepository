using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Devabit.Telelingua.ReportingServices.DAL.Helpers;
using Devabit.Telelingua.ReportingServices.DAL.Interface;
using Devabit.Telelingua.ReportingServices.Enums;
using Devabit.Telelingua.ReportingServices.Helpers;
using Devabit.Telelingua.ReportingServices.Models.DataModels;
using Devabit.Telelingua.ReportingServices.Calculation;
using Devabit.Telelingua.ReportingServices.DataAccess;

namespace Devabit.Telelingua.ReportingServices.DAL.Implementation
{
    /// <summary>
    /// The implementation of ITableDal
    /// </summary>
    public class TableDal : ITableDal
    {
        #region Fields
        private readonly ConfigurationService config;

        private readonly QueryDbContext context;
        #endregion

        #region Constructor
        public TableDal(ConfigurationService config, QueryDbContext context)
        {
            this.config = config;
            this.context = context;
        }

        #endregion

        #region ITableDal implementation

        /// <inheritdoc/>
        public IEnumerable<TableViewModel> GetTableNames()
        {
            var res = new List<TableViewModel>();

            using (var connection = new SqlConnection(config.ConnectionString))
            {
                connection.Open();

                var command = new SqlCommand(SqlExpression.GetTableNameExpression, connection);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        res.Add(new TableViewModel()
                        {
                            Name = reader.GetString(1),
                            Schema = reader.GetString(0)
                        });
                    }
                    reader.Close();
                }
                connection.Close();
            }

            return res.OrderBy(x => x.Name);
        }

        /// <inheritdoc/>
        public TableDataModel GetTable(TableViewModel table)
        {
            using (var connection = new SqlConnection(config.ConnectionString))
            {
                connection.Open();
                var adapter = new SqlDataAdapter(new SqlCommand(SqlExpression.GetTableInfoExpression, connection));
                adapter.SelectCommand.Parameters.Add(new SqlParameter("@p1", table.Name));
                adapter.SelectCommand.Parameters.Add(new SqlParameter("@p2", table.Schema));
                var set = new DataSet();
                adapter.Fill(set);
                var columns = new Dictionary<string, ColumnTypes>();
                using (var reader = set.CreateDataReader())
                {
                    while (reader.Read())
                    {
                        columns.Add(reader.GetString(0), EnumConverter.ConvertToColumnType(reader.GetString(1)));
                    }
                    reader.Close();
                }
                connection.Close();

                return new TableDataModel()
                {
                    TableId = table.Id,
                    Name = $"{table.Schema}.{table.Name}",
                    Columns = columns.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value)
                };
            }
        }

        public int GetRecordsCount(QueryModel query, string connectionStringName = null)
        {
            var selectModels = GetConvertedQueries(query.TableQueries);
            if(selectModels.All(sm=>sm.SelectedColumns.Count == 0))
            {
                return 0;
            }
            var connectionString = connectionStringName == null ? config.ConnectionString : config.GetUserConnectionString(connectionStringName);
           // var connectionString = "Data Source = 193.93.216.233;Initial Catalog = TILT2_DEV;Integrated Security = False;User Id = sae;Password=Devabit1@";

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var sqlSelectCommand = selectModels.CreateBaseSqlSelectCommand(query.SelectDistinct);
                sqlSelectCommand.ExtendSqlQueryWithFilters(query.Filters, selectModels);
                sqlSelectCommand.ChangeSelectedColumnsIfGroupBy();
                var totalCountQueryResult =
                    new SqlCommand(SqlExpression.CountExpression(sqlSelectCommand.ToString()), connection)
                        .ExecuteScalar();
                return (int?)totalCountQueryResult ?? 0;
            }
        }

        public string GetSqlScriptFromQueryModel(PagedQueryModel query)
        {
            var script = string.Empty;
            for(int i=0; i< query.Queries.Count;i++)
            {
                var selectModels = GetConvertedQueries(query.Queries[i].TableQueries);
                var sqlSelectCommand = selectModels.CreateBaseSqlSelectCommand(query.Queries[i].SelectDistinct);
                sqlSelectCommand.ExtendSqlQueryWithFilters(query.Queries[i].Filters, selectModels);
                sqlSelectCommand.ChangeSelectedColumnsIfGroupBy();
                if (i == query.Queries.Count - 1)
                {
                    sqlSelectCommand.ExtendSqlQueryWithOrderBy(selectModels);
                }
                script+= sqlSelectCommand.ToString();
                if (i<query.Queries.Count-1 && query.Queries.Count > 1)
                {
                    script += "\n union \n";
                }
            }
            return script;
        }

        ///<inheritdoc/>
        public PagedQueryResultModel ProcessQuery(PagedQueryModel query,
            string connectionStringName = null,
            List<ComparableFieldModel> comparisons = null,
            List<CalculatedColumnModel> externalCalculations = null)
        {
            try
            {
                if(query.Queries.All(q=>q.TableQueries.All(tq=>tq.SelectedColumns.Count == 0 && tq.Functions.Count == 0)))
                {
                    return new PagedQueryResultModel
                    {
                        PageNumber=1,
                        Result = new QueryResultModel
                        {
                            ColumnHeaders = new List<HeaderModel>(),
                            Rows = new List<RowModel>()
                        }
                    };
                }
                var connectionString = "Data Source = 193.93.216.233;Initial Catalog = TILT2_DEV;Integrated Security = False;User Id = sae;Password=Devabit1@";
                //var connectionString = connectionStringName == null ? config.ConnectionString : config.GetUserConnectionString(connectionStringName);

                var selectModels = GetConvertedQueries(query.Queries[0].TableQueries);
                var offset = (query.PagingModel.PageNumber - 1) * query.PagingModel.PageSize + query.PagingModel.Skip;
                var pageSize = query.PagingModel.PageSize;

                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    var sqlSelectCommand = selectModels.CreateBaseSqlSelectCommand(query.Queries[0].SelectDistinct);
                    sqlSelectCommand.ExtendSqlQueryWithFilters(query.Queries[0].Filters,selectModels);
                    sqlSelectCommand.ChangeSelectedColumnsIfGroupBy();
                    var totalCountQueryResult = new SqlCommand(SqlExpression.CountExpression(sqlSelectCommand.ToString()), connection).ExecuteScalar();
                    var totalCount = (int?)totalCountQueryResult ?? 0;
                    if (query.PagingModel.PageNumber == 0)
                    {
                        return new PagedQueryResultModel()
                        {
                            Result = new QueryResultModel()
                            {
                                Rows = new List<RowModel>()
                            },
                            TotalCount = (int?)totalCountQueryResult ?? 0,
                            PageCount = (long)Math.Ceiling((double)totalCount / query.PagingModel.PageSize),
                            PageNumber = query.PagingModel.PageNumber,
                            HasNext = Math.Ceiling((double)totalCount / query.PagingModel.PageSize) > query.PagingModel.PageNumber,
                        };
                    }
                    sqlSelectCommand.ExtendSqlQueryWithPagination(selectModels, offset, pageSize);
                    var queryResult = ProcessQueryResult(sqlSelectCommand.ToString(), connection);

                    if (sqlSelectCommand.HasGroupBy)
                    {
                        sqlSelectCommand.ChangeSelectedColumnsIfGroupBy(true);
                        var selectString = sqlSelectCommand.ToString();
                        var groupDetailsQueryResult = ProcessQueryResult(sqlSelectCommand.ToString(), connection);
                        queryResult.SetInternalRows(groupDetailsQueryResult, sqlSelectCommand.GroupByColumns.Select(x => x.Split('.')[1]).ToList());
                    }
                    connection.Close();
                    var pagedQueryResult = new PagedQueryResultModel()
                    {
                        TotalCount = totalCount,
                        PageCount = (long)Math.Ceiling((double)totalCount / query.PagingModel.PageSize),
                        PageNumber = query.PagingModel.PageNumber,
                        HasNext = Math.Ceiling((double)totalCount / query.PagingModel.PageSize) > query.PagingModel.PageNumber,
                        Result = queryResult
                    };

                if (queryResult.Rows.Count != 0)
                {
                   // query.Query.PrepareCalculations();
                    // ProcessCalculatedColumns(pagedQueryResult, query);
                    CalculationProcessor.ProcessCalculatedColumns(pagedQueryResult, query.Queries[0].CalculatedColumns);
                }

                    //comparisons
                    if (sqlSelectCommand.HasGroupBy && queryResult.Rows.Count != 0 && comparisons != null)
                    {
                        var groupings = sqlSelectCommand.GroupByColumns
                                                        .Select(x => x.Split('.')[1])
                                                        .ToList();
                        comparisons.Reverse();
                        foreach (var comparison in comparisons)
                        {
                            ChangeFilters(comparison, query.Queries[0]);
                            var temp = this.ProcessQuery(query, query.ConnectionStringName);
                            totalCount = temp.TotalCount > totalCount ? temp.TotalCount : totalCount;
                            var columnIndex = queryResult.ColumnHeaders.FindIndex(x => x.Name == comparison.Column.ColumnAlias);

                            pagedQueryResult.Result.ExtendResultWithComparisons(temp.Result, groupings, columnIndex);

                            pagedQueryResult.Result.ColumnHeaders.Insert(columnIndex + 1, new HeaderModel
                            {
                                Name = comparison.Alias,
                                Type = pagedQueryResult.Result.ColumnHeaders[columnIndex].Type
                            });
                        }
                    }

                    if (externalCalculations != null)
                    {
                        CalculationProcessor.ProcessCalculatedColumns(pagedQueryResult, externalCalculations);
                    }
                    
                    foreach (var row in pagedQueryResult.Result.Rows)
                    {
                        if (!query.Queries[0].ShowDetails)
                        {
                            row.Internal = null;
                        }
                        else if (row.Internal != null)
                        {
          //                  row.Internal.Rows = row.Internal.Rows.Take(100).ToList();
                        }
                    }

                    return pagedQueryResult;
                }
            }
            catch(Exception e)
            {
                context.Errors.Add(new DataAccess.Models.ErrorQuery()
                {
                    QueryText = Newtonsoft.Json.JsonConvert.SerializeObject(query),
                    Date = DateTime.Now,
                    Exception = e.Message
                });
                context.SaveChanges();
                throw;
            }
        }

        public PagedQueryResultModel ProcessSqlScript(SqlScriptModel sqlScript)
        {
            var sqlRequest = sqlScript.SqlRequest;
            AddParameters(sqlScript);
            var connectionString = string.IsNullOrEmpty(sqlRequest.ConnectionStringName) ? config.ConnectionString : config.GetUserConnectionString(sqlRequest.ConnectionStringName);
            //var connectionString = "Data Source = 193.93.216.233;Initial Catalog = TILT2_DEV;Integrated Security = False;User Id = sae;Password=Devabit1@";
            var offset = (sqlRequest.PaginationModel.PageNumber - 1) * sqlRequest.PaginationModel.PageSize + sqlRequest.PaginationModel.Skip;
            var pageSize = sqlRequest.PaginationModel.PageSize;
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                //var totalCount = new SqlCommand(SqlExpression.CountExpression(sqlRequest.SqlScript), connection).ExecuteScalar();
               // sqlRequest.SqlScript.AddPaginationToScript(offset, pageSize);
                var sqlCommand = new SqlCommand(sqlRequest.SqlScript, connection);
                var sqlDataAdapter = new SqlDataAdapter(sqlCommand);
                var dataSet = new DataSet();
                sqlDataAdapter.Fill(dataSet);
                var parser = new DataSetParser(dataSet);
                var result = parser.ToQueryResultModel();
                var totalCount = result.Rows.Count;
                result.Rows = result.Rows.Skip(Convert.ToInt32(offset)).Take(Convert.ToInt32(pageSize)).ToList();
                return new PagedQueryResultModel
                {
                    TotalCount = totalCount,// (int?)totalCount ?? 0,
                    PageCount = (long)Math.Ceiling((double)totalCount / sqlRequest.PaginationModel.PageSize),
                    PageNumber = sqlRequest.PaginationModel.PageNumber,
                    HasNext = Math.Ceiling((double)totalCount / sqlRequest.PaginationModel.PageSize) > sqlRequest.PaginationModel.PageNumber,
                    Result = result
                };
            }
        }
        #endregion

        #region Helpers

        private void AddParameters(SqlScriptModel sqlScript)
        {
            foreach (var parameter  in sqlScript.Parameters)
            {
                sqlScript.SqlRequest.SqlScript = sqlScript.SqlRequest.SqlScript.Replace($"[@{parameter.Name}@]", parameter.Value);
            }
        }
        /// <summary>
        /// Gets converted queries. Modification of getSelectments() method
        /// </summary>
        /// <param name="queries">List of queries, where each item is query to separate table.</param>
        /// <returns>List of selectments model.</returns>
        private List<SelectModel> GetConvertedQueries(List<QueryTableModel> queries)
        {
            var tables = queries.Select(query => GetTable(new TableViewModel {Id = query.Id, Name = query.TableName, Schema = query.TableSchema })).ToList();

            var queryConverter = new QueryConverter(tables);
            var convertedQueries = queries.Select(query => queryConverter.ToSelectModel(query)).ToList();
            //var lastContainingWhere = convertedQueries.LastOrDefault(x => x.Wheres.Any());
            //if (lastContainingWhere != null)
            //{
            //    queryConverter.CorrectLastWhere(ref lastContainingWhere);
            //}
            return convertedQueries.ToList();
        }

        /// <summary>
        /// Processes query script
        /// </summary>
        /// <param name="script">script text</param>
        /// <param name="connection">connection to DB</param>
        /// <returns></returns>
        private QueryResultModel ProcessQueryResult(string script, SqlConnection connection, int resultRowsNumber = 0)
        {
            var sqlDataAdapter = new SqlDataAdapter(new SqlCommand(script, connection));
            var dataSet = new DataSet();
            try
            {
                sqlDataAdapter.Fill(dataSet);
            }
            catch (Exception e)
            {
                throw new BadRequestException("Query can`t be processed");
            }
            var dataSetParser = new DataSetParser(dataSet);
            return dataSetParser.ToQueryResultModel(resultRowsNumber);
        }

        private void ChangeFilters(ComparableFieldModel comparison, QueryModel query)
        {



                query.Filters.RemoveAll(filter => filter.IsExternal);
                query.Filters.AddRange(comparison.Filters
                                                      .Select(filter => new WhereGroup
                                                      {
                                                          IsExternal = true,
                                                          WhereGroups = new List<WhereGroup>(),
                                                          WhereStatements = new List<WhereModel>
                                                          {
                                                              new WhereModel
                                                              {
                                                                  TableId = query.TableQueries.FirstOrDefault(x=>x.TableName==filter.TableName).Id,
                                                                  ColumnAlias = filter.ColumnName,
                                                                  Value = filter.Value,
                                                                  Comparison = filter.ComparisonType,
                                                                  UseOr=false
                                                              }
                                                          }
                                                      }));
            
            query.ShowDetails = false;
        }


        /// <summary>
        /// Gets list of fields needed for calculation.
        /// </summary>
        /// <param name="calculation">The calculation to be parsed.</param>
        /// <returns>List of fields needed for calculation.</returns>
        private List<string> GetFieldsForCalculations(CalculatedColumnEntity calculation)
        {
            var res = new List<string>();
            if (calculation.EntityType == CalculatedEntityType.Column || calculation.EntityType == CalculatedEntityType.Function)
            {
                res.Add(calculation.Value);
            }

            if (calculation.LeftOp != null)
            {
                res = res.Concat(GetFieldsForCalculations(calculation.LeftOp)).ToList();
            }

            if (calculation.RightOp != null)
            {
                res = res.Concat(GetFieldsForCalculations(calculation.RightOp)).ToList();
            }

            return res;
        }

      

       
        #endregion

        /// TODO : Remove
        ///<inheritdoc/>
        //public PagedQueryResultModel ProcessQuery(PagedQueryModel query, List<ComparableFieldModel> comparisons = null)
        //{
        //    using (var connection = new SqlConnection(config.ConnectionString))
        //    {
        //        connection.Open();
        //        if (query.Query.TableQueries.All(x => x.GroupByColumns.Count == 0))
        //        {
        //            var selectments = getSelectments(query.Query.TableQueries);

        //            var distinctString = query.Query.SelectDistinct ? "distinct" : "";
        //            var columnsString = string.Join(',', selectments.SelectMany(x => x.SelectedColumns));
        //            var functionsString = string.Join(',', selectments.SelectMany(x => x.Functions));
        //            var selectString = functionsString != "" ? string.Join(',', columnsString, functionsString) : columnsString;
        //            var tablesString = selectments.Select(x => x.TableName + " " + x.TableAlias).First();
        //            var joinsString = string.Join('\n', selectments.SelectMany(x => x.Joins));
        //            var whereString = selectments.Any(x => x.Wheres.Count != 0) && !selectments.SelectMany(x => x.GroupBys).Any()
        //                ? $"where {string.Join(" ", selectments.SelectMany(x => x.Wheres))}"
        //                : "";
        //            var orderString = selectments.Any(x => x.OrderBys.Count > 0) ? $"order by {string.Join(',', selectments.SelectMany(x => x.OrderBys))}" : $"order by {selectments[0].SelectedColumns[0]}";
        //            var offsetString = $"OFFSET {(query.PagingModel.PageNumber - 1) * query.PagingModel.PageSize} ROWS";
        //            var fetchString = $"FETCH NEXT {query.PagingModel.PageSize} ROWS ONLY";

        //            var command = $"select {distinctString} {selectString} " +
        //                $"from {tablesString}" +
        //                $" {joinsString}" +
        //                $" {whereString}";

        //            var countString =
        //                "select distinct COUNT(*) over() as count" +
        //                $" from ({command}) subb";

        //            var countRes = new SqlCommand(countString, connection).ExecuteScalar();
        //            var count = 0;
        //            if (countRes != null)
        //                count = (int)countRes;

        //            var adapter = new SqlDataAdapter(new SqlCommand(
        //                command +
        //                $"{orderString}" +
        //                $" {offsetString}" +
        //                $" {fetchString}",
        //                connection));

        //            var set = new DataSet();
        //            try
        //            {
        //                adapter.Fill(set);
        //            }
        //            catch
        //            {
        //                throw new BadRequestException("Query can`t be processed");
        //            }

        //            var result = new QueryResultModel()
        //            {
        //                ColumnHeaders = new List<HeaderModel>(),
        //                Rows = new List<RowModel>()
        //            };
        //            using (var reader = set.CreateDataReader())
        //            {
        //                var schema = reader.GetSchemaTable();
        //                foreach (DataRow column in schema.Rows)
        //                {
        //                    var type = (Type)column.ItemArray[5];
        //                    var typeString = type == typeof(string) ? "string" :
        //                        type == typeof(float) ? "float" :
        //                        type == typeof(DateTime) ? "date" : "int";
        //                    result.ColumnHeaders.Add(new HeaderModel()
        //                    {
        //                        Name = column.ItemArray[0].ToString(),
        //                        Type = typeString
        //                    });
        //                }

        //                while (reader.Read())
        //                {
        //                    var row = (new RowModel()
        //                    {
        //                        Values = new List<string>()
        //                    });
        //                    var i = 0;
        //                    foreach (var unused in result.ColumnHeaders)
        //                    {
        //                        row.Values.Add(reader.GetValue(i++).ToString());
        //                    }
        //                    result.Rows.Add(row);
        //                    if (result.Rows.Count >= 100) break;
        //                }
        //                reader.Close();
        //            }
        //            connection.Close();


        //            var r = new PagedQueryResultModel()
        //            {
        //                TotalCount = count,
        //                PageCount = (long)Math.Ceiling((double)count / query.PagingModel.PageSize),
        //                PageNumber = query.PagingModel.PageNumber,
        //                HasNext = Math.Ceiling((double)count / query.PagingModel.PageSize) > query.PagingModel.PageNumber,
        //                Result = result
        //            };
        //            if (result.Rows.Count != 0)
        //            {
        //                ProcessCalculatedColumns(r, query);
        //            }
        //            return r;
        //        }

        //        {
        //            var selectments = getSelectments(query.Query.TableQueries);

        //            if (!query.Query.ShowDetails)
        //            {
        //                var t = new List<string>();
        //                t = query.Query.CalculatedColumns.Select(x => x.Calculation)
        //                    .Aggregate(t, (current, calculation) => current.Concat(GetFieldsForCalculations(calculation)).Distinct().ToList());
        //                foreach (var select in selectments)
        //                {
        //                    select.SelectedColumns.RemoveAll(x =>
        //                    {
        //                        var columnName = x.Split('.')[1];
        //                        return !t.Contains(columnName);
        //                    });
        //                }
        //            }

        //            var distinctString = query.Query.SelectDistinct ? "distinct" : "";
        //            var columnsString = string.Join(',', selectments.SelectMany(x => x.SelectedColumns));
        //            var functionsString = string.Join(',', selectments.SelectMany(x => x.Functions));
        //            var tablesString = selectments.Select(x => x.TableName + " " + x.TableAlias).First();
        //            var joinsString = string.Join('\n', selectments.SelectMany(x => x.Joins));
        //            var whereString = selectments.Any(x => x.Wheres.Count != 0) ?/*selectments.Any(x => x.Wheres.Count != 0) && !selectments.SelectMany(x => x.GroupBys).Any()*/
        //                                                                         /*?*/ $"where {string.Join(" ", selectments.SelectMany(x => x.Wheres))}" : ""
        //                /*: ""*/;
        //            var orderString = selectments.Any(x => x.OrderBys.Count > 0) ? $"order by {string.Join(',', selectments.SelectMany(x => x.OrderBys))}" : $"order by {selectments.SelectMany(x => x.GroupBys).First()}";
        //            var offsetString = $"OFFSET {(query.PagingModel.PageNumber - 1) * query.PagingModel.PageSize} ROWS";
        //            var fetchString = $"FETCH NEXT {query.PagingModel.PageSize} ROWS ONLY";

        //            var groupString = selectments.SelectMany(x => x.GroupBys).Any()
        //                ? $"group by {string.Join(',', selectments.SelectMany(x => x.GroupBys))}" : "";
        //            var havingString =
        //                //selectments.Any(x => x.Wheres.Count != 0) && selectments.SelectMany(x => x.GroupBys).Any()
        //                //    ? $"having {string.Join(" and ", selectments.SelectMany(x => x.Wheres))}"
        //                //    : 
        //                "";
        //            var selectString = string.Join(',', selectments.SelectMany(x => x.GroupBys));
        //            selectString = functionsString != ""
        //                ? string.Join(',', selectString, functionsString)
        //                : selectString;

        //            // var selColumns = selectments.SelectMany(x => x.GroupBys)
        //            // .Concat(selectments.SelectMany(x => x.Functions)).ToList();

        //            var groupings = selectments.SelectMany(x => x.GroupBys).ToList();

        //            var command = $"select {distinctString} {selectString} " +
        //                          $"from {tablesString}" +
        //                          $" {joinsString}" +
        //                          $" {whereString}" +
        //                          $" {groupString}" +
        //                          $" {havingString}";


        //            var countString =
        //                "select distinct COUNT(*) over() as count" +
        //                $" from ({command}) subb";

        //            var countRes = new SqlCommand(countString, connection).ExecuteScalar();
        //            var count = 0;
        //            if (countRes != null)
        //                count = (int)countRes;

        //            var adapter = new SqlDataAdapter(
        //                new SqlCommand(
        //                    $" {command}" +
        //                    $" {orderString}" +
        //                    $" {offsetString}" +
        //                    $" {fetchString}",
        //                    connection));

        //            var set = new DataSet();
        //            try
        //            {
        //                adapter.Fill(set);
        //            }
        //            catch
        //            {
        //                throw new BadRequestException("Query can`t be processed");
        //            }

        //            var result = new QueryResultModel()
        //            {
        //                ColumnHeaders = new List<HeaderModel>(),
        //                Rows = new List<RowModel>()
        //            };

        //            var groupingsString = string.Join(',', groupings);
        //            if (columnsString == "")
        //            {
        //                columnsString = groupingsString;
        //            }
        //            else if (groupingsString != "")
        //            {
        //                columnsString = string.Join(',', groupingsString, columnsString);

        //            }

        //            var subSelect = $"select {distinctString} {columnsString} " +
        //                            $"from {tablesString} " +
        //                            $" {joinsString} " +
        //                            $" {whereString} " +
        //                            $" {orderString} ";

        //            var inner = new QueryResultModel
        //            {
        //                ColumnHeaders = new List<HeaderModel>(),
        //                Rows = new List<RowModel>()
        //            };

        //            var innerAdapter = new SqlDataAdapter(subSelect, connection);
        //            var innerSet = new DataSet();
        //            innerAdapter.Fill(innerSet);

        //            using (var innerReader = innerSet.CreateDataReader())
        //            {

        //                var innerSchema = innerReader.GetSchemaTable();
        //                foreach (DataRow column in innerSchema.Rows)
        //                {
        //                    var type = (Type)column.ItemArray[5];
        //                    var typeString = type == typeof(string) ? "string" :
        //                        type == typeof(float) ? "float" :
        //                        type == typeof(DateTime) ? "date" : "int";
        //                    inner.ColumnHeaders.Add(new HeaderModel()
        //                    {
        //                        Name = column.ItemArray[0].ToString(),
        //                        Type = typeString
        //                    });
        //                }

        //                while (innerReader.Read())
        //                {
        //                    var innerRow = new RowModel()
        //                    {
        //                        Values = new List<string>()
        //                    };
        //                    var j = 0;

        //                    foreach (var unused in inner.ColumnHeaders)
        //                    {
        //                        innerRow.Values.Add(innerReader.GetValue(j++).ToString());
        //                    }

        //                    inner.Rows.Add(innerRow);
        //                }
        //                innerReader.Close();
        //            }
        //            using (var reader = set.CreateDataReader())
        //            {
        //                var schema = reader.GetSchemaTable();
        //                foreach (DataRow column in schema.Rows)
        //                {
        //                    var type = (Type)column.ItemArray[5];
        //                    var typeString = type == typeof(string) ? "string" :
        //                        type == typeof(float) ? "float" :
        //                        type == typeof(DateTime) ? "date" : "int";
        //                    result.ColumnHeaders.Add(new HeaderModel()
        //                    {
        //                        Name = column.ItemArray[0].ToString(),
        //                        Type = typeString
        //                    });
        //                }

        //                while (reader.Read())
        //                {
        //                    var row = (new RowModel()
        //                    {
        //                        Values = new List<string>()
        //                    });

        //                    var i = 0;

        //                    foreach (var unused in result.ColumnHeaders)
        //                    {
        //                        row.Values.Add(reader.GetValue(i++).ToString());
        //                    }

        //                    row.Internal = new QueryResultModel
        //                    {
        //                        ColumnHeaders = inner.ColumnHeaders,
        //                        Rows = inner.Rows.Where(x =>
        //                        {
        //                            var ind = 0;
        //                            foreach (var unused in groupings)
        //                            {
        //                                if (row.Values[ind] != x.Values[ind]) return false;
        //                                ind++;
        //                            }
        //                            return true;
        //                        }).ToList()
        //                    };
        //                    result.Rows.Add(row);

        //                    if (result.Rows.Count >= 100)
        //                    {
        //                        break;
        //                    }

        //                }

        //                reader.Close();
        //            }
        //            connection.Close();

        //            var r = new PagedQueryResultModel()
        //            {
        //                TotalCount = count,
        //                PageCount = (long)Math.Ceiling((double)count / query.PagingModel.PageSize),
        //                PageNumber = query.PagingModel.PageNumber,
        //                HasNext = Math.Ceiling((double)count / query.PagingModel.PageSize) > query.PagingModel.PageNumber,
        //                Result = result
        //            };
        //            if (result.Rows.Count != 0)
        //            {
        //                ProcessCalculatedColumns(r, query);
        //            }

        //            foreach (var row in r.Result.Rows)
        //            {
        //                if (!query.Query.ShowDetails)
        //                {
        //                    row.Internal = null;
        //                }
        //                else
        //                {
        //                    row.Internal.Rows = row.Internal.Rows.Take(100).ToList();
        //                }
        //            }
        //            return r;
        //        }
        //    }
        //}


        //<summary>
        // Gets the parsed values from queries.
        //</summary>
        //<param name = "queries" > List of query table models, where each item represents queries to one table.</param>
        //<returns>The list of selectments models.</returns>
        //private List<SelectModel> getSelectments(List<QueryTableModel> queries)
        //{
        //    var i = 0;
        //    var res = queries.Select(x => new SelectModel()
        //    {
        //        TableName = x.TableSchema + "." + x.TableName,
        //        TableAlias = $"t{i++}"
        //    }).ToList();

        //    foreach (var query in queries)
        //    {
        //        var model = res.FirstOrDefault(x => x.TableName == query.TableSchema + "." + query.TableName);
        //        if (model == null) continue;
        //        {
        //            model.SelectedColumns = query.SelectedColumns.Select(x => model.TableAlias + "." + x).ToList();
        //            model.Joins = query.Joins.Select(join =>
        //            {
        //                var second = join.ToTable.Schema + "." + join.ToTable.Name != model.TableName && res.FirstOrDefault(x =>
        //                                        x.TableName == @join.ToTable.Schema + "." + @join.ToTable.Name) != null ? res.FirstOrDefault(x =>
        //                           x.TableName == @join.ToTable.Schema + "." + @join.ToTable.Name) : new SelectModel()
        //                           {
        //                               TableName = @join.ToTable.Schema + "." + @join.ToTable.Name,
        //                               TableAlias = $"t{res.Count + 1}"
        //                           };
        //                var joinFrom = $"{model.TableAlias}.{@join.FromColumn}";
        //                if (join.TrimFirst)
        //                {
        //                    joinFrom = $"left({joinFrom}, {join.TrimFirstBy})";
        //                }

        //                var joinTo = $"{second.TableAlias}.{join.OnColumn}";
        //                if (join.TrimSecond)
        //                {
        //                    joinTo = $"left({joinTo}, {join.TrimSecondBy})";
        //                }
        //                return
        //                    $"left join {second.TableName} {second.TableAlias} " +
        //                    $"on {joinFrom} {@join.Comparison}" +
        //                    $"{joinTo} ";
        //            }).ToList();

        //            model.Wheres = query.Filters.SelectMany(x => GetWhereGroup(x, model.TableAlias)).ToList();

        //            model.OrderBys = query.Sortings
        //                .Select(sort => $"{model.TableAlias}.{sort.OrderByColumn} {sort.Direction}").ToList();
        //            model.GroupBys = query.GroupByColumns.Select(x => $"{model.TableAlias}.{x}").ToList();
        //            model.Functions = query.Functions.Select(x => $"{x.Function}({model.TableAlias}.{x.Column}) as {x.Alias}")
        //                .ToList();
        //        }
        //    }

        //    var lastContainingWhere = res.LastOrDefault(x => x.Wheres.Any());
        //    if (lastContainingWhere != null)
        //    {
        //        var last = lastContainingWhere.Wheres.Last();
        //        var resultString = last.Remove(last.LastIndexOf(')') + 1);
        //        lastContainingWhere.Wheres[lastContainingWhere.Wheres.Count - 1] = resultString;
        //    }
        //    return res;
        //}

        ///// <summary>
        ///// Gets where statements for where groups.
        ///// </summary>
        ///// <param name="group">Group to be parsed.</param>
        ///// <param name="tableAlias">Alias of the table for where statement.</param>
        ///// <returns>List of where statements.</returns>
        //private List<string> GetWhereGroup(WhereGroup group, string tableAlias)
        //{
        //    var res = new List<string>();
        //    for (var index = 0; index < @group.WhereStatements.Count; index++)
        //    {
        //        var statement = "";
        //        if (index == 0)
        //        {
        //            statement = "(";
        //        }
        //        var @where = @group.WhereStatements[index];
        //        var match = Regex.Match(where.ColumnName, @"year\((.*)\)");
        //        var useAlias = true;
        //        if (match.Success)
        //        {
        //            where.ColumnName = where.ColumnName.Replace(match.Groups[1].Value, tableAlias + "." + match.Groups[1].Value);
        //            useAlias = false;
        //        }
        //        if (@where.CompareTo == "null")
        //        {
        //            switch (@where.Comparison)
        //            {
        //                case "=":
        //                    @where.Comparison = " is ";
        //                    break;
        //                case "<>":
        //                    @where.Comparison = "is not ";
        //                    break;
        //            }
        //        }

        //        else
        //        {
        //            @where.CompareTo = $"\'{@where.CompareTo.Replace("'", "''")}\'";
        //        }
        //        if (useAlias)
        //            statement += $"{tableAlias}.{where.ColumnName} {where.Comparison} {where.CompareTo} ";
        //        else
        //        {
        //            statement += $"{where.ColumnName} {where.Comparison} {where.CompareTo} ";

        //        }

        //        if (where.UseOr)
        //        {
        //            statement += " or ";
        //        }
        //        else
        //        {
        //            statement += " and ";
        //        }
        //        res.Add(statement);
        //    }

        //    res = res.Concat(group.WhereGroups.SelectMany(x => GetWhereGroup(x, tableAlias)).ToList()).ToList();
        //    var last = res.Last();
        //    var lastOr = last.LastIndexOf("or");
        //    var lastAnd = last.LastIndexOf("and");
        //    last = last.Insert(lastOr > lastAnd ? lastOr : lastAnd, ")");
        //    res[res.Count - 1] = last;
        //    return res;
        //}


    }
}