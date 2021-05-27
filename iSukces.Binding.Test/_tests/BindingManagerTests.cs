using System.Globalization;
using System.Text;
using iSukces.Binding.Test.Data;
using Xunit;

namespace iSukces.Binding.Test
{
    public class BindingManagerTests
    {
        [Theory]
        [InlineData("dispose listener")]
        [InlineData("dispose binding manager")]
        public void T01_Should_bind_change_and_dispose(string testNumber)
        {
            var bm  = new BindingManager();
            var log = new StringBuilder();

            var data = new SimpleNpc(log, "Obj");
            data.Title = "initial value";
            Assert.False(data.HasPropertyChangedSubscribers);

            var binding = bm.From(data);
            Assert.False(data.HasPropertyChangedSubscribers);
            binding.WithPath(nameof(data.Title));
            Assert.False(data.HasPropertyChangedSubscribers);
            int changessCount = 0;
            var listener = binding.CreateListener((q, kind) =>
            {
                log.AppendLine($"Got value {kind}: '{q}'");
                changessCount++;
            });
            Assert.Equal(1, changessCount);
            Assert.True(data.HasPropertyChangedSubscribers);
            data.Title = "new Value";
            Assert.Equal(2, changessCount);

            if (testNumber == "dispose listener")
                listener.Dispose();
            if (testNumber == "dispose binding manager")
                bm.Dispose();

            Assert.False(data.HasPropertyChangedSubscribers);

            const string expected = @"
[Obj] Subscribe PropertyChanged
Got value StartBinding: 'initial value'
Got value ValueChanged: 'new Value'
Got value EndBinding: 'Unbound'
[Obj] Unubscribe PropertyChanged
";
            var log1 = log.ToString().Trim();
            Assert.Equal(expected.Trim(), log1);
        }


        [Fact]
        public void T02_Should_update_source()
        {
            var bm  = new BindingManager();
            var log = new StringBuilder();

            var data    = new SimpleNpc(log, "Obj") {IntNumber = 99};
            var binding = bm.From(data, q => q.IntNumber);
            Assert.False(data.HasPropertyChangedSubscribers);

            var changessCount = 0;
            var updater = binding.CreateTwoWayBinding<int>((q, kind) =>
            {
                if (q is not BindingSpecial)
                {
                    Assert.True(q is int);
                }

                log.AppendLine($"Got value {kind}: '{q}'");
                changessCount++;
            });

            data.IntNumber = 13;
            updater.UpdateSource(27);
            Assert.Equal(27, data.IntNumber);

            Assert.Equal(3, changessCount);
            const string expected = @"
[Obj] Subscribe PropertyChanged
Got value StartBinding: '99'
Got value ValueChanged: '13'
Got value UpdateSource: '27'
";
            var log1 = log.ToString().Trim();
            Assert.Equal(expected.Trim(), log1);
        }

        [Fact]
        public void T03_Should_not_update_invalid_value()
        {
            var bm  = new BindingManager();
            var log = new StringBuilder();

            var data    = new SimpleNpc(log, "Obj") {IntNumber = 99};
            var binding = bm.From(data, q => q.IntNumber);
            Assert.False(data.HasPropertyChangedSubscribers);

            var changessCount = 0;
            var updater = binding.CreateTwoWayBinding<int>((q, kind) =>
            {
                log.AppendLine($"Got value {kind}: '{q}'");
                changessCount++;
            });

            data.IntNumber = 13;
            var updateResult = updater.UpdateSource("27");
            Assert.Equal(UpdateSourceResult.InvalidValue, updateResult);
            Assert.Equal(2, changessCount);
            const string expected = @"
[Obj] Subscribe PropertyChanged
Got value StartBinding: '99'
Got value ValueChanged: '13'
";
            var log1 = log.ToString().Trim();
            Assert.Equal(expected.Trim(), log1);
        }

        [Fact]
        public void T04_Should_update_value_with_conversion()
        {
            var bm  = new BindingManager();
            var log = new StringBuilder();

            var data    = new SimpleNpc(log, "Obj") {IntNumber = 99};
            var binding = bm.From(data, q => q.IntNumber);
            binding.Converter = NumberValueConverter.Instance;
            Assert.False(data.HasPropertyChangedSubscribers);

            var changessCount = 0;
            var updater = binding.CreateTwoWayBinding<int, string>((q, kind) =>
            {
                if (q is not BindingSpecial)
                {
                    Assert.True(q is null or string);
                }

                log.AppendLine($"Got value {kind}: '{q}'");
                changessCount++;
            });

            data.IntNumber = 13;
            var updateResult = updater.UpdateSource("27");
            Assert.Equal(UpdateSourceResult.Ok, updateResult);
            Assert.Equal(27, data.IntNumber);

            Assert.Equal(3, changessCount);
            const string expected = @"
[Obj] Subscribe PropertyChanged
Got value StartBinding: '99'
Got value ValueChanged: '13'
Got value UpdateSource: '27'
";
            var log1 = log.ToString().Trim();
            Assert.Equal(expected.Trim(), log1);
        }


        [Theory]
        [InlineData("en-US", "$99.45", "$27.44")]
        [InlineData("en-GB", "£99.45", "£27.44")]
        [InlineData("pl-PL", "99,45 zł", "27,44 zł")]
        [InlineData("fr-Fr", "99,45 €", "27,44 €")]
        public void T05_Should_update_value_with_conversion_and_culture(string culture, string expected1,
            string expected2)
        {
            var bm  = new BindingManager();
            var log = new StringBuilder();

            var data    = new SimpleNpc(log, "Obj") {DecimalNumber = 99.45m};
            var binding = bm.From(data, q => q.DecimalNumber);
            binding.Converter          = NumberValueConverter.Instance;
            binding.CultureInfo        = CultureInfo.GetCultureInfo(culture);
            binding.ConverterParameter = "c2";
            Assert.False(data.HasPropertyChangedSubscribers);

            var changessCount = 0;
            var updater = binding.CreateTwoWayBinding<decimal, string>((q, kind) =>
            {
                if (q is not BindingSpecial)
                {
                    Assert.True(q is null or string);
                }

                log.AppendLine($"Got value {kind}: '{q}'");
                changessCount++;
            });

            var stringNumber = 27.44.ToString(binding.CultureInfo);
            var updateResult = updater.UpdateSource(stringNumber);
            Assert.Equal(UpdateSourceResult.Ok, updateResult);
            Assert.Equal(27.44m, data.DecimalNumber);

            Assert.Equal(2, changessCount);
            string expected = @$"
[Obj] Subscribe PropertyChanged
Got value StartBinding: '{expected1}'
Got value UpdateSource: '{expected2}'
";
            var log1 = log.ToString().Trim();
            Assert.Equal(expected.Trim(), log1);
        }
        
        
        [Fact]
        public void T06_Should_catch_update_source_exception()
        {
            var bm  = new BindingManager();
            var log = new StringBuilder();

            var data    = new SimpleNpc(log, "Obj") {DecimalNumber = 99.45m};
            var binding = bm.From(data, q => q.DecimalNumber);
            Assert.False(data.HasPropertyChangedSubscribers);

            var changessCount = 0;
            var updater = binding.CreateTwoWayBinding<decimal, string>((q, kind) =>
            {
                if (q is not BindingSpecial)
                {
                    Assert.True(q is decimal);
                }

                log.AppendLine($"Got value {kind}: '{q}'");
                changessCount++;
            });

            var updateResult = updater.UpdateSource(-1.345m);
            Assert.NotNull(updateResult.Exception);
            Assert.Equal(99.45m, data.DecimalNumber);
            Assert.Equal(2, changessCount);
            bm.Dispose();
            const string expected = @"
[Obj] Subscribe PropertyChanged
Got value StartBinding: '99,45'
Got value UpdateSource: 'Invalid'
Got value EndBinding: 'Unbound'
[Obj] Unubscribe PropertyChanged
";
            var log1 = log.ToString().Trim();
            Assert.Equal(expected.Trim(), log1);
        }
    }
}
