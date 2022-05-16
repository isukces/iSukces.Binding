using System;

namespace iSukces.Binding
{
    public sealed class RangeBindingValidator : BindingValidator
    {
        public RangeBindingValidator(double? min, double? max)
        {
            _min = min;
            _max = max;
        }

        private static string NotGreaterThan(object compareTo)
        {
            return $"Wartość nie może być większa niż {compareTo}.";
        }

        private static string NotLowerThan(object compareTo)
        {
            return $"Wartość nie może być mniejsza niż {compareTo}.";
        }


        public override BindingValidatorResult Check(object value)
        {
            switch (value)
            {
                case int intValue:
                {
                    if (_min.HasValue)
                    {
                        var compareTo = (int)Math.Round(_min.Value);
                        if (intValue < compareTo)
                            return NotLowerThan(compareTo);
                    }

                    if (_max.HasValue)
                    {
                        var compareTo = (int)Math.Round(_max.Value);
                        if (intValue > compareTo)
                            return NotGreaterThan(compareTo);
                    }

                    return BindingValidatorResult.Success;
                }
                case double doubleValue:
                {
                    if (_min.HasValue)
                    {
                        var compareTo = _min.Value;
                        if (doubleValue < compareTo)
                            return NotLowerThan(compareTo);
                    }

                    if (_max.HasValue)
                    {
                        var compareTo = (int)Math.Round(_max.Value);
                        if (doubleValue > compareTo)
                            return NotGreaterThan(compareTo);
                    }

                    return BindingValidatorResult.Success;
                }
                case null: return BindingValidatorResult.Success;
            }

            throw new NotImplementedException();
        }

        private readonly double? _max;
        private readonly double? _min;
    }
}
