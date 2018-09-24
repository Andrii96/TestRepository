using Devabit.Telelingua.ReportingServices.Models.DataModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Devabit.Telelingua.ReportingServices.DAL.Helpers
{
    public class DataSetParser
    {
        #region Properties
        /// <summary>
        /// data set to be parsed
        /// </summary>
        public DataSet DataSet { get; set; }

        #endregion

        #region Constructors
        public DataSetParser() { }
        public DataSetParser(DataSet set)
        {
            DataSet = set;
        }
        #endregion

        #region Methods

        public QueryResultModel ToQueryResultModel(int rowsNumber = 0)
        {
            using (var reader = DataSet.CreateDataReader())
            {
                var columnsInfo = GetColumnsInfo(reader);
                var rows = GetParsedRows(reader, columnsInfo.Count, rowsNumber);
                reader.Close();
                return new QueryResultModel
                {
                    ColumnHeaders = columnsInfo,
                    Rows = rows
                };
            }
        }

        #endregion

        #region Helpers
        private List<HeaderModel> GetColumnsInfo(DataTableReader reader)
        {
            var columnsInfo = new List<HeaderModel>();
            var schema = reader.GetSchemaTable();
            foreach (DataRow columnInfo in schema.Rows)
            {
                var columnName = columnInfo.ItemArray[0].ToString();
                var columnType = columnInfo.ItemArray[5] as Type;
                var columnTypeName = columnType == typeof(string) ? "string" :
                                     columnType == typeof(float) ? "float" :
                                     columnType == typeof(bool) ? "bool" :
                                     columnType == typeof(DateTime) ? "date" : "int";
                columnsInfo.Add(new HeaderModel
                {
                    Name = columnName,
                    Type = columnTypeName
                });
            }
            return columnsInfo;
        }

        private List<RowModel> GetParsedRows(DataTableReader reader, int columnsNumber, int rowsNumber = 0)
        {
            var rows = new List<RowModel>();
            while (reader.Read())
            {
                var rowValues = new List<string>();
                var count = 0;
                while (count < columnsNumber)
                {
                    rowValues.Add(reader.GetValue(count++).ToString().Replace('\n', ' ').Replace('\r',' ').Replace('\t', ' ').Replace(';',','));
                }
                rows.Add(new RowModel
                {
                    Values = rowValues
                });
                if (rowsNumber > 0 && rows.Count >= rowsNumber) break;
            }
            return rows;
        }
        #endregion
    }
}
