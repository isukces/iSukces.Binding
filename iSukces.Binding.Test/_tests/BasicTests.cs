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
                new object[] {1, typeof(object), Bla.Acceptable},
                new object[] {1, typeof(long), Bla.NeedConversion},
                new object[] {1, typeof(IConvertible), Bla.Acceptable},

                new object[] {null, typeof(object), Bla.Acceptable},
                new object[] {null, typeof(IConvertible), Bla.Acceptable},
                new object[] {null, typeof(long), Bla.NeedConversion}
            };
        }

        [Theory]
        [MemberData(nameof(Dane_do_testowania))]
        public void MetodaTestowa(object value, Type requestedType, Bla expected)
        {
            Assert.Equal(expected, Tools.NeedsConversion(value, requestedType));
        }
    }
}
