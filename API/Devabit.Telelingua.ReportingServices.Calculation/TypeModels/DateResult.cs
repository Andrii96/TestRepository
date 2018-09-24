using System;

namespace Devabit.Telelingua.ReportingServices.Calculation.TypeModels
{
    /// <summary>
    /// CalculationResult representing dates.
    /// </summary>
    public class DateResult : CalculationResult
    {
        public DateResult(string value)
        {
            if (DateTime.TryParse(value, out _))
            {
                this.Value = value;
            }
            else
            {
                throw new Exception("Types mismatch");
            }
        }

        public DateResult(DateTime value)
        {
            this.Value = value.ToString();
        }

        #region Add

        public override CalculationResult Add(StringResult second)
        {
            return new StringResult(this.Value + second.Value);
        }

        public override CalculationResult Add(NumberResult second)
        {
            throw new Exception("Can`t add date and number");
        }

        public override CalculationResult Add(DateResult second)
        {
            return new DateResult(DateTime.Parse(this.Value) + new TimeSpan(DateTime.Parse(second.Value).Ticks));

        }

        public override CalculationResult Add(BoolResult second)
        {
            throw new Exception("Can`t add date and bool.");
        }

        #endregion

        #region Substract

        public override CalculationResult Substract(StringResult second)
        {
            throw new Exception("Can`t substract Date and String.");
        }

        public override CalculationResult Substract(NumberResult second)
        {
            throw new Exception("Can`t substract Date and Number.");
        }

        public override CalculationResult Substract(DateResult second)
        {
            return new DateResult(DateTime.Parse(this.Value) - new TimeSpan(DateTime.Parse(second.Value).Ticks));
        }

        public override CalculationResult Substract(BoolResult second)
        {
            throw new Exception("Can`t substract bools.");
        }
        #endregion

        #region Multiply

        public override CalculationResult Multiply(StringResult second)
        {
            throw new Exception("Can`t multiply dates.");
        }

        public override CalculationResult Multiply(NumberResult second)
        {
            throw new Exception("Can`t multiply dates.");
        }

        public override CalculationResult Multiply(DateResult second)
        {
            throw new Exception("Can`t multiply dates.");
        }

        public override CalculationResult Multiply(BoolResult second)
        {
            throw new Exception("Can`t multiply dates.");
        }
        #endregion

        #region Divide

        public override CalculationResult Divide(StringResult second)
        {
            throw new Exception("Can`t divide dates.");
        }

        public override CalculationResult Divide(NumberResult second)
        {
            throw new Exception("Can`t divide dates.");
        }

        public override CalculationResult Divide(DateResult second)
        {
            throw new Exception("Can`t divide dates.");
        }

        public override CalculationResult Divide(BoolResult second)
        {
            throw new Exception("Can`t divide dates.");
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
            return new BoolResult(false);
        }

        public override BoolResult IsBigger(DateResult second)
        {
            return new BoolResult(DateTime.Parse(this.Value) > DateTime.Parse(second.Value));
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
            return new BoolResult(true);
        }

        public override BoolResult IsLess(DateResult second)
        {
            return new BoolResult(DateTime.Parse(this.Value) < DateTime.Parse(second.Value));
        }

        public override BoolResult IsLess(BoolResult second)
        {
            return new BoolResult(false);
        }
        #endregion
    }
}
