using System;

namespace Devabit.Telelingua.ReportingServices.Calculation.TypeModels
{
    /// <summary>
    /// Base class for calculation results.
    /// </summary>
    public abstract class CalculationResult
    {
        public string Value { get; set; }

        protected CalculationResult()
        {
        }

        protected CalculationResult(string value)
        {
            Value = value;
        }

        public override string ToString()
        {
            return this.Value;
        }

        public string GetResultType()
        {
            switch (this)
            {
                case StringResult _:
                    return "string";
                case NumberResult _:
                    return "float";
                case DateResult _:
                    return "date";
                case BoolResult _:
                    return "bool";
            }
            return "calculated";
        }

        #region Actions

        internal CalculationResult Add<T>(T second) where T : CalculationResult
        {
            switch (second)
            {
                case StringResult _:
                    return Add(second as StringResult);
                case NumberResult _:
                    return Add(second as NumberResult);
                case DateResult _:
                    return Add(second as DateResult);
                case BoolResult _:
                    return Add(second as BoolResult);
            }

            return this;
        }

        internal CalculationResult Substract<T>(T second) where T : CalculationResult
        {
            switch (second)
            {
                case StringResult _:
                    return Substract(second as StringResult);
                case NumberResult _:
                    return Substract(second as NumberResult);
                case DateResult _:
                    return Substract(second as DateResult);
                case BoolResult _:
                    return Substract(second as BoolResult);
            }

            return this;
        }

        internal CalculationResult Multiply<T>(T second) where T : CalculationResult
        {
            switch (second)
            {
                case StringResult _:
                    return Multiply(second as StringResult);
                case NumberResult _:
                    return Multiply(second as NumberResult);
                case DateResult _:
                    return Multiply(second as DateResult);
                case BoolResult _:
                    return Multiply(second as BoolResult);
            }

            return this;
        }

        internal CalculationResult Divide<T>(T second) where T : CalculationResult
        {
            switch (second)
            {
                case StringResult _:
                    return Divide(second as StringResult);
                case NumberResult _:
                    return Divide(second as NumberResult);
                case DateResult _:
                    return Divide(second as DateResult);
                case BoolResult _:
                    return Divide(second as BoolResult);
            }

            return this;
        }

        public BoolResult Equals<T>(T second) where T : CalculationResult
        {
            return new BoolResult(this.Value == second.Value);
        }

        public BoolResult NotEquals<T>(T second) where T : CalculationResult
        {
            return new BoolResult(this.Value != second.Value);
        }

        internal BoolResult IsBigger<T>(T second) where T : CalculationResult
        {
            switch (second)
            {
                case StringResult _:
                    return IsBigger(second as StringResult);
                case NumberResult _:
                    return IsBigger(second as NumberResult);
                case DateResult _:
                    return IsBigger(second as DateResult);
                case BoolResult _:
                    return IsBigger(second as BoolResult);
            }

            return null;
        }

        internal BoolResult IsLess<T>(T second) where T : CalculationResult
        {
            switch (second)
            {
                case StringResult _:
                    return IsLess(second as StringResult);
                case NumberResult _:
                    return IsLess(second as NumberResult);
                case DateResult _:
                    return IsLess(second as DateResult);
                case BoolResult _:
                    return IsLess(second as BoolResult);
            }

            return null;
        }

        public BoolResult And<T>(T second) where T : CalculationResult
        {
            if (this is BoolResult && second is BoolResult)
            {
                return new BoolResult(bool.Parse(this.Value) && bool.Parse(second.Value));
            }
            throw new System.Exception("Can`t apply and to non logical operations;");
        }

        public BoolResult Or<T>(T second) where T : CalculationResult
        {
            if (this is BoolResult && second is BoolResult)
            {
                return new BoolResult(bool.Parse(this.Value) || bool.Parse(second.Value));
            }
            throw new System.Exception("Can`t apply or to non logical operations;");
        }

        public CalculationResult GetYear()
        {
            if (string.IsNullOrEmpty(this.Value))
            {
                return new NumberResult(0);
            }
            if (!(this is DateResult))
            {
                throw new System.Exception("Can`t get year: unsupported type");
            }

            return new NumberResult(DateTime.Parse(this.Value).Year);
        }

        public CalculationResult GetMonth()
        {
            if (string.IsNullOrEmpty(this.Value))
            {
                return new NumberResult(0);
            }
            if (!(this is DateResult))
            {
                throw new System.Exception("Can`t get month: unsupported type");
            }
            return new NumberResult(DateTime.Parse(this.Value).Month);
        }

        public CalculationResult GetDay()
        {
            if (string.IsNullOrEmpty(this.Value))
            {
                return new NumberResult(0);
            }
            if (!(this is DateResult))
            {
                throw new System.Exception("Can`t get day: unsupported type");
            }
            return new NumberResult(DateTime.Parse(this.Value).Day);
        }

        public CalculationResult GetDate()
        {
            if (string.IsNullOrEmpty(this.Value))
            {
                return new StringResult("");
            }
            if (!(this is DateResult))
            {
                throw new System.Exception("Can`t get day: unsupported type");
            }
            return new DateResult(DateTime.Parse(this.Value).Date);
        }

        public CalculationResult DateDiff<T>(T second) where T : CalculationResult
        {
            if (string.IsNullOrEmpty(this.Value) || string.IsNullOrEmpty(second.Value))
            {
                return new StringResult("");
            }
            if (!(this is DateResult) || !(second is DateResult))
            {
                throw new System.Exception("Can`t get date: unsupported type");
            }

            var span = DateTime.Parse(this.Value).Subtract(DateTime.Parse(second.Value));

            return new NumberResult(span.Days);
        }

        #endregion


        #region Add

        public abstract CalculationResult Add(StringResult second);

        public abstract CalculationResult Add(NumberResult second);

        public abstract CalculationResult Add(DateResult second);

        public abstract CalculationResult Add(BoolResult second);


        #endregion

        #region Substract

        public abstract CalculationResult Substract(StringResult second);
        public abstract CalculationResult Substract(NumberResult second);
        public abstract CalculationResult Substract(DateResult second);
        public abstract CalculationResult Substract(BoolResult second);

        #endregion

        #region Multiply

        public abstract CalculationResult Multiply(StringResult second);
        public abstract CalculationResult Multiply(NumberResult second);
        public abstract CalculationResult Multiply(DateResult second);
        public abstract CalculationResult Multiply(BoolResult second);

        #endregion

        #region Divide

        public abstract CalculationResult Divide(StringResult second);
        public abstract CalculationResult Divide(NumberResult second);
        public abstract CalculationResult Divide(DateResult second);
        public abstract CalculationResult Divide(BoolResult second);

        #endregion

        #region IsBigger

        public abstract BoolResult IsBigger(StringResult second);
        public abstract BoolResult IsBigger(NumberResult second);
        public abstract BoolResult IsBigger(DateResult second);
        public abstract BoolResult IsBigger(BoolResult second);

        #endregion

        #region IsLess

        public abstract BoolResult IsLess(StringResult second);
        public abstract BoolResult IsLess(NumberResult second);
        public abstract BoolResult IsLess(DateResult second);
        public abstract BoolResult IsLess(BoolResult second);

        #endregion
    }
}
