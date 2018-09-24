using System;
using System.Collections.Generic;
using System.Linq;
using Devabit.Telelingua.ReportingServices.Calculation.TypeModels;
using Devabit.Telelingua.ReportingServices.Enums;
using Devabit.Telelingua.ReportingServices.Helpers;
using Devabit.Telelingua.ReportingServices.Models.DataModels;

namespace Devabit.Telelingua.ReportingServices.Calculation
{
    public static class CalculationParser
    {
        /// <summary>
        /// Processes single calculation
        /// </summary>
        /// <param name="calculation">The calculation to be processed.</param>
        /// <param name="headers">Headers for row to be processed.</param>
        /// <param name="row">The row to be processed.</param>
        /// <returns>The calculation result.</returns>
        public static CalculationResult ProcessCalculation(CalculatedColumnEntity calculation, List<HeaderModel> headers, RowModel row)
        {
            switch (calculation.EntityType)
            {
                case Enums.CalculatedEntityType.CalculatedEntity:
                    return ProcessComplexCalculation(calculation, headers, row);
                case Enums.CalculatedEntityType.Column:
                    return ProcessColumn(calculation, headers, row);
                case Enums.CalculatedEntityType.Function:
                    return ProcessFunction(calculation, headers, row);
                case Enums.CalculatedEntityType.Value:
                    return ProcessValue(calculation);
                default:
                    return null;
            }
        }

        /// <summary>
        /// Checks if calculation is related to grouped data or to internal data.
        /// </summary>
        /// <param name="calculation">The calculation to be checked.</param>
        /// <param name="groupHeaders">Headers of group data.</param>
        /// <returns><c>true</c> if calculation is related to group; otherwise <c>false</c></returns>
        public static bool IsGroupWide(this CalculatedColumnEntity calculation, List<HeaderModel> groupHeaders)
        {
            if (calculation == null) return false;
            if (calculation.EntityType == CalculatedEntityType.Function) return true;
            if (calculation.EntityType == CalculatedEntityType.Column &&
                groupHeaders.Any(x => x.Name == calculation.Value)) return true;
            return calculation.LeftOp.IsGroupWide(groupHeaders) ||
                    calculation.RightOp.IsGroupWide(groupHeaders);
        }

        /// <summary>
        /// Processes value calculations.
        /// </summary>
        /// <param name="calculation">The calculation to be processed.</param>
        /// <returns>The calculation result.</returns>
        private static CalculationResult ProcessValue(CalculatedColumnEntity calculation)
        {
            return ProcessValue(calculation.Value);
        }

        /// <summary>  
        /// Processes value calculations.
        /// </summary>
        /// <param name="calculation">The calculation to be processed.</param>
        /// <returns>The calculation result.</returns>
        private static CalculationResult ProcessValue(string value)
        {
            if (bool.TryParse(value, out _)) return new BoolResult(value);
            else if (DateTime.TryParse(value, out _)) return new DateResult(value);
            else if (double.TryParse(value, out _)) return new NumberResult(value);
            else return new StringResult(value);
        }

        /// <summary>
        /// Processes the aggregate function calculation. 
        /// </summary> 
        /// <param name="calculation">The calculation to be processed.</param>
        /// <param name="headers">Headers for row to be processed.</param>
        /// <param name="row">The row to be processed.</param>
        /// <returns>The calculation result.</returns>
        private static CalculationResult ProcessFunction(CalculatedColumnEntity calculation, List<HeaderModel> headers, RowModel row)
        {
            var index = row.Internal?.ColumnHeaders.FindIndex(x => x.Name == calculation.Value) ?? headers.FindIndex(x => x.Name == calculation.Value);
            var value = GetFromHeader(headers[index], row.Values[index]);
            var values = row.Internal != null ? row.Internal.Rows.Select(x => GetFromHeader(row.Internal.ColumnHeaders[index], x.Values[index])).ToList() : new List<CalculationResult>();
            switch (calculation.Action)
            {
                case "sum":
                    return (values.Aggregate((i, j) => i.Add(j)));
                case "max":
                    return (values.Aggregate((i, j) => bool.Parse(i.IsBigger(j).Value) ? i : j));
                case "min":
                    return (values.Aggregate((i, j) => bool.Parse(i.IsLess(j).Value) ? i : j));
                case "avg":
                    return values.Aggregate((i, j) => i.Add(j)).Divide(new NumberResult(values.Count));
                case "year":
                    return value.GetYear();
                case "month":
                    return value.GetMonth();
                case "day":
                    return value.GetDay();
                case "date":
                    return value.GetDate();
            }
            throw new BadRequestException("Unrecognized function");
        }

        /// <summary>
        /// Processes the column calculation.
        /// </summary>
        /// <param name="calculation">The calculation to be processed.</param>
        /// <param name="headers">Headers for row to be processed.</param>
        /// <param name="row">The row to be processed.</param>
        /// <returns>The calculation result.</returns>
        private static CalculationResult ProcessColumn(CalculatedColumnEntity calculation, List<HeaderModel> headers, RowModel row)
        {
            var index = headers.FindIndex(x => x.Name == calculation.Value);
            if (index != -1) return GetFromHeader(headers[index], row.Values[index]);
            throw new BadRequestException("No such column found.");
        }

        /// <summary>
        /// Processes the complex calculation.
        /// </summary>
        /// <param name="calculation">The calculation to be processed.</param>
        /// <param name="headers">Headers for row to be processed.</param>
        /// <param name="row">The row to be processed.</param>
        /// <returns>The calculation result.</returns>
        private static CalculationResult ProcessComplexCalculation(CalculatedColumnEntity calculation, List<HeaderModel> headers, RowModel row)
        {
            var left = ProcessCalculation(calculation.LeftOp, headers, row);
            var right = calculation.RightOp != null ? ProcessCalculation(calculation.RightOp, headers, row) : null;

            switch (calculation.Action)
            {
                case "+":
                    {
                        return left.Add(right);
                    }

                case "-":
                    {
                        return left.Substract(right);
                    }

                case "*":
                    {
                        return left.Multiply(right);
                    }

                case "/":
                    {
                        return left.Divide(right);
                    }

                case "=":
                    {
                        return left.Equals(right);
                    }
                case "<>":
                case "!":
                    {
                        return left.NotEquals(right);
                    }
                case ">":
                    {
                        return left.IsBigger(right);
                    }
                case "<":
                    {
                        return left.IsLess(right);
                    }
                case "&&":
                    {
                        return left.And(right);
                    }
                case "||":
                    {
                        return left.Or(right);
                    }
                case "datediff":
                    {
                        return left.DateDiff(right);
                    }
                case "replace":
                    {
                        if (left is BoolResult && bool.Parse(left.Value))
                        {
                            return right;
                        }
                        else
                        {
                            return left;
                        }
                    }
                case "replaceIf":
                    {
                        return ProcessReplaceIf(calculation, headers, row);
                    }
            }

            throw new BadRequestException("Unsupported action");
        }

        /// <summary>
        /// Gets the calculation result based on header model and value.
        /// </summary>
        /// <param name="header">The header model used to get result`s type.</param>
        /// <param name="value">The value of result.</param>
        /// <returns>The parsed value.</returns>
        private static CalculationResult GetFromHeader(HeaderModel header, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return new StringResult(value);
            }
            if (header.Type == "string")
            {
                return new StringResult(value);
            }
            if (header.Type == "float" || header.Type == "int")
            {
                return new NumberResult(value);
            }
            if (header.Type == "bool")
            {
                return new BoolResult(value);
            }
            if (header.Type == "date")
            {
                return new DateResult(value);
            }
            throw new BadRequestException("Unexpected column type");
        }

        /// <summary>
        /// Processes replace if calculations.
        /// </summary>
        /// <param name="calculation">The calculation to be processed.</param>
        /// <param name="headers">Headers for row to be processed.</param>
        /// <param name="row">The row to be processed.</param>
        /// <returns>The calculation result.</returns>
        private static CalculationResult ProcessReplaceIf(CalculatedColumnEntity calculation, List<HeaderModel> headers, RowModel row)
        {
            var left = ProcessCalculation(calculation.LeftOp, headers, row);
            foreach (var option in calculation.ReplaceIfOptions)
            {
                var compareTo = ProcessValue(option.CompareTo);
                switch (option.Comparison)
                {
                    case "=":
                        {
                            if (bool.Parse(left.Equals(compareTo).Value))
                            {
                                return ProcessCalculation(option.ReplaceWith, headers, row);
                            }
                            break;
                        }
                    case "!=":
                    case "<>":
                        {
                            if (bool.Parse(left.NotEquals(compareTo).Value))
                            {
                                return ProcessCalculation(option.ReplaceWith, headers, row);
                            }
                            break;
                        }
                    case ">":
                        {
                            if (bool.Parse(left.IsBigger(compareTo).Value))
                            {
                                return ProcessCalculation(option.ReplaceWith, headers, row);
                            }
                            break;
                        }
                    case "<":
                        {
                            if (bool.Parse(left.IsLess(compareTo).Value))
                            {
                                return ProcessCalculation(option.ReplaceWith, headers, row);
                            }
                            break;
                        }
                }
            }
            return left;
        }
    }
}