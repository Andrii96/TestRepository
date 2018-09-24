using System;
using System.Linq;

namespace Devabit.Telelingua.ReportingServices.Calculation.TypeModels
{
    /// <summary>
    /// CalculationResult representing string values.
    /// </summary>
    public class StringResult : CalculationResult
    {
        public StringResult(string value) : base(value)
        {
        }

        #region Add

        public override CalculationResult Add(StringResult second)
        {
            return new StringResult(this.Value + second.Value);
        }

        public override CalculationResult Add(NumberResult second)
        {
            return new StringResult(this.Value + second.Value);
        }

        public override CalculationResult Add(DateResult second)
        {
            return new StringResult(this.Value + second.Value);
        }

        public override CalculationResult Add(BoolResult second)
        {
            return new StringResult(this.Value + second.Value);
        }

        #endregion

        #region Substract

        public override CalculationResult Substract(StringResult second)
        {
            throw new Exception("Can`t substract Strings.");
        }

        public override CalculationResult Substract(NumberResult second)
        {
            if (string.IsNullOrEmpty(this.Value))
            {
                return new NumberResult(0 - double.Parse(second.Value));
            }
            if (double.TryParse(this.Value, out _))
            {
                return new NumberResult(double.Parse(Value) - double.Parse(second.Value));
            }
            throw new Exception("Can`t substract Strings.");
        }

        public override CalculationResult Substract(DateResult second)
        {
            throw new Exception("Can`t substract Strings.");
        }

        public override CalculationResult Substract(BoolResult second)
        {
            throw new Exception("Can`t substract Strings.");
        }
        #endregion

        #region Multiply

        public override CalculationResult Multiply(StringResult second)
        {
            throw new Exception("Can`t multiply strings.");
        }

        public override CalculationResult Multiply(NumberResult second)
        {
            if (int.TryParse(second.Value, out var res))
            {
                return new StringResult(string.Join("", Enumerable.Repeat(this.Value, res)));
            }
            else
            {
                throw new Exception("Cant multiply number and string.");
            }
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
            throw new Exception("Can`t divide strings.");
        }

        public override CalculationResult Divide(NumberResult second)
        {
            if (string.IsNullOrEmpty(this.Value)) return new NumberResult(0);
            throw new Exception("Can`t divide strings.");
        }

        public override CalculationResult Divide(DateResult second)
        {
            throw new Exception("Can`t divide strings.");
        }

        public override CalculationResult Divide(BoolResult second)
        {
            throw new Exception("Can`t divide strings.");
        }
        #endregion

        #region IsBigger
        public override BoolResult IsBigger(StringResult second)
        {
            if (string.IsNullOrEmpty(this.Value))
            {
                return new BoolResult(false);
            }
            return new BoolResult(string.Compare(this.Value, second.Value) == 1);
        }

        public override BoolResult IsBigger(NumberResult second)
        {
            if (string.IsNullOrEmpty(this.Value))
            {
                return new BoolResult(false);
            }
            return new BoolResult(true);
        }

        public override BoolResult IsBigger(DateResult second)
        {
            if (string.IsNullOrEmpty(this.Value))
            {
                return new BoolResult(false);
            }
            return new BoolResult(true);
        }

        public override BoolResult IsBigger(BoolResult second)
        {
            if (string.IsNullOrEmpty(this.Value))
            {
                return new BoolResult(false);
            }
            return new BoolResult(true);
        }
        #endregion

        #region IsLess
        public override BoolResult IsLess(StringResult second)
        {
            if (string.IsNullOrEmpty(this.Value))
            {
                return new BoolResult(false);
            }
            return new BoolResult(string.Compare(this.Value, second.Value) == -1);
        }

        public override BoolResult IsLess(NumberResult second)
        {
            if (string.IsNullOrEmpty(this.Value))
            {
                return new BoolResult(false);
            }
            return new BoolResult(false);
        }

        public override BoolResult IsLess(DateResult second)
        {
            if (string.IsNullOrEmpty(this.Value))
            {
                return new BoolResult(false);
            }
            return new BoolResult(false);
        }

        public override BoolResult IsLess(BoolResult second)
        {
            if (string.IsNullOrEmpty(this.Value))
            {
                return new BoolResult(false);
            }
            return new BoolResult(false);
        }
        #endregion

    }
}
