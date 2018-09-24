using System;

namespace Devabit.Telelingua.ReportingServices.Calculation.TypeModels
{
    /// <summary>
    /// CalculationResult representing booleans.
    /// </summary>
    public class BoolResult : CalculationResult
    {
        public BoolResult(string value)
        {
            if (value == "1") value = "True";
            if (value == "0") value = "False";
            if (bool.TryParse(value, out _))
            {
                this.Value = value;
            }
            else
            {
                throw new Exception("Types mismatch");
            }
        }

        public BoolResult(bool value)
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
            throw new Exception("Can`t add bools.");
        }

        public override CalculationResult Add(DateResult second)
        {
            throw new Exception("Can`t add bools.");
        }

        public override CalculationResult Add(BoolResult second)
        {
            throw new Exception("Can`t add bools.");
        }
        #endregion

        #region Substract

        public override CalculationResult Substract(StringResult second)
        {
            throw new Exception("Can`t substract bools.");
        }

        public override CalculationResult Substract(NumberResult second)
        {
            throw new Exception("Can`t substract bools.");
        }

        public override CalculationResult Substract(DateResult second)
        {
            throw new Exception("Can`t substract bools.");
        }

        public override CalculationResult Substract(BoolResult second)
        {
            throw new Exception("Can`t substract bools.");
        }
        #endregion

        #region Multiply

        public override CalculationResult Multiply(StringResult second)
        {
            throw new Exception("Can`t multiply bools.");
        }

        public override CalculationResult Multiply(NumberResult second)
        {
            throw new Exception("Can`t multiply bools.");
        }

        public override CalculationResult Multiply(DateResult second)
        {
            throw new Exception("Can`t multiply bools.");
        }

        public override CalculationResult Multiply(BoolResult second)
        {
            throw new Exception("Can`t multiply bools.");
        }
        #endregion

        #region Divide

        public override CalculationResult Divide(StringResult second)
        {
            throw new Exception("Can`t divide bools.");
        }

        public override CalculationResult Divide(NumberResult second)
        {
            throw new Exception("Can`t divide bools.");
        }

        public override CalculationResult Divide(DateResult second)
        {
            throw new Exception("Can`t divide bools.");
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
            return new BoolResult(false);
        }

        public override BoolResult IsBigger(DateResult second)
        {
            return new BoolResult(false);
        }

        public override BoolResult IsBigger(BoolResult second)
        {
            if (bool.Parse(this.Value) == true && bool.Parse(second.Value) == false)
            {
                return new BoolResult(true);
            }
            else
            {
                return new BoolResult(false);
            }
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
            return new BoolResult(true);
        }

        public override BoolResult IsLess(BoolResult second)
        {
            if (bool.Parse(this.Value) == false && bool.Parse(second.Value) == true)
            {
                return new BoolResult(true);
            }
            else
            {
                return new BoolResult(false);
            }
        }
        #endregion

    }
}
