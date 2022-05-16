using System;
using System.Collections.Generic;
using Xunit;

namespace iSukces.Binding.Test
{
    public class BasicTests
    {
        public static IList<object[]> Dane_do_testowania()
        {
            return new[]
            {
                new object[] {1, typeof(object), ValueConversionStatus.Acceptable},
                new object[] {1, typeof(long), ValueConversionStatus.NeedConversion},
                new object[] {1, typeof(IConvertible), ValueConversionStatus.Acceptable},

                new object[] {null, typeof(object), ValueConversionStatus.Acceptable},
                new object[] {null, typeof(IConvertible), ValueConversionStatus.Acceptable},
                new object[] {null, typeof(long), ValueConversionStatus.NeedConversion}
            };
        }

        [Theory]
        [MemberData(nameof(Dane_do_testowania))]
        public void MetodaTestowa(object value, Type requestedType, ValueConversionStatus expected)
        {
            Assert.Equal(expected, Tools.NeedsConversion(value, requestedType));
        }
    }
}
