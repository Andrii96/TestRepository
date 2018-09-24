using System;
using System.Linq;

namespace Devabit.Telelingua.ReportingServices.Calculation.TypeModels
{
    /// <summary>
    /// CalculationResult representing numerical values.
    /// </summary>
    public class NumberResult : CalculationResult
    {
        public NumberResult(string value)
        {
            if (double.TryParse(value, out _))
            {
                this.Value = value;
            }
            else
            {
                throw new Exception("Type mismatch");
            }
        }

        public NumberResult(double value)
        {
            this.Value = value.ToString();
        }

        public override string ToString()
        {
            return String.Format("{0:0.00}", double.Parse(this.Value));
        }

        #region Add

        public override CalculationResult Add(StringResult second)
        {
            return new StringResult(this.Value + second.Value);
        }

        public override CalculationResult Add(NumberResult second)
        {
            return new NumberResult(double.Parse(this.Value) + double.Parse(second.Value));
        }

        public override CalculationResult Add(DateResult second)
        {
            throw new Exception("Can`t add number and date.");
        }

        public override CalculationResult Add(BoolResult second)
        {
            throw new Exception("Can`t add number and bool.");
        }

        #endregion

        #region Substract

        public override CalculationResult Substract(StringResult second)
        {
            if (string.IsNullOrEmpty(second.Value))
            {
                return this;
            }
            throw new Exception("Can`t substract Number and String.");
        }

        public override CalculationResult Substract(NumberResult second)
        {
            return new NumberResult(double.Parse(this.Value) - double.Parse(second.Value));
        }

        public override CalculationResult Substract(DateResult second)
        {
            throw new Exception("Can`t substract number and string");
        }

        public override CalculationResult Substract(BoolResult second)
        {
            throw new Exception("Can`t substract bools.");
        }
        #endregion

        #region Multiply

        public override CalculationResult Multiply(StringResult second)
        {
            if (int.TryParse(this.Value, out var res))
            {
                return new StringResult(string.Join("", Enumerable.Repeat(second.Value, res)));
            }
            else
            {
                throw new Exception("Cant multiply number and string.");
            }
        }

        public override CalculationResult Multiply(NumberResult second)
        {
            return new NumberResult(double.Parse(this.Value) * double.Parse(second.Value));
        }

        public override CalculationResult Multiply(DateResult second)
        {
            throw new Exception("Can`t multiply dates.");
        }

        public override CalculationResult Multiply(BoolResult second)
        {
            throw new Exception("Can`t multiply bools.");
        }
        #endregion

        #region Divide

        public override CalculationResult Divide(StringResult second)
        {
            return new NumberResult(0);
            throw new Exception("Can`t divide strings.");
        }

        public override CalculationResult Divide(NumberResult second)
        {
            var secondValue = double.Parse(second.Value);
            if (secondValue == 0)
            {
                return new NumberResult(0);
            }
            return new NumberResult(double.Parse(this.Value) / secondValue);
        }

        public override CalculationResult Divide(DateResult second)
        {
            throw new Exception("Can`t divide dates.");
        }

        public override CalculationResult Divide(BoolResult second)
        {
            throw new Exception("Can`t divide bools.");
        }
        #endregion

        #region IsBigger
        public override BoolResult IsBigger(StringResult second)
        {
            if (string.IsNullOrEmpty(second.Value))
            {
                return new BoolResult(true);
            }
            return new BoolResult(false);
        }

        public override BoolResult IsBigger(NumberResult second)
        {
            return new BoolResult(double.Parse(this.Value) > double.Parse(second.Value));
        }

        public override BoolResult IsBigger(DateResult second)
        {
            return new BoolResult(true);
        }

        public override BoolResult IsBigger(BoolResult second)
        {
            return new BoolResult(true);
        }
        #endregion

        #region IsLess
        public override BoolResult IsLess(StringResult second)
        {
            if (string.IsNullOrEmpty(second.Value))
            {
                return new BoolResult(true);
            }
            return new BoolResult(true);
        }

        public override BoolResult IsLess(NumberResult second)
        {
            return new BoolResult(double.Parse(this.Value) < double.Parse(second.Value));
        }

        public override BoolResult IsLess(DateResult second)
        {
            return new BoolResult(false);
        }

        public override BoolResult IsLess(BoolResult second)
        {
            return new BoolResult(false);
        }
        #endregion
    }
}
