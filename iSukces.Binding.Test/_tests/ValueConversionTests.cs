using System.Globalization;
using iSukces.Binding.Test.Data;
using Xunit;

namespace iSukces.Binding.Test
{
    public class ValueConversionTests
    {
        [Fact]
        public void T01_Should_convert_with_type_converter()
        {
            var src    = new WrappedInt(5);
            var result = DefaultValueConverter.Instance.Convert(src, typeof(int), null, CultureInfo.InvariantCulture);
            Assert.True(result is int);
            Assert.Equal(5, (int)result);
        }

        [Fact]
        public void T02_Should_convert_back_with_type_converter()
        {
            var src = 5;
            var result =
                DefaultValueConverter.Instance.ConvertBack(src, typeof(WrappedInt), null, CultureInfo.InvariantCulture);
            Assert.True(result is WrappedInt);
            Assert.Equal(5, ((WrappedInt)result).Value);
        }

        [Fact]
        public void T03_Should_convert_with_type_converter_oposite_converter_use()
        {
            var src = new WrappedInt(5);
            var result =
                DefaultValueConverter.Instance.ConvertBack(src, typeof(int), null, CultureInfo.InvariantCulture);
            Assert.True(result is int);
            Assert.Equal(5, (int)result);
        }

        [Fact]
        public void T04_Should_convert_back_with_type_converter_oposite_converter_use()
        {
            var src = 5;
            var result =
                DefaultValueConverter.Instance.Convert(src, typeof(WrappedInt), null, CultureInfo.InvariantCulture);
            Assert.True(result is WrappedInt);
            Assert.Equal(5, ((WrappedInt)result).Value);
        }
    }
}
