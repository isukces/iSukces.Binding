using iSukces.Binding.Test.Data;
using Xunit;
using Xunit.Abstractions;

namespace iSukces.Binding.Test
{
    public class UpdaterTests
    {
        public UpdaterTests(ITestOutputHelper testOutputHelper) { _testOutputHelper = testOutputHelper; }


        [Fact]
        public void T01_Should_update_null_target()
        {
            var testing = new TestingTool(_testOutputHelper);
            testing.bm.PropertyInfoProviderRegistry = new SimplePropertyInfoProviderRegistry(true);
            var info = testing.bm.PropertyInfoProviderRegistry.FindProvider(null, "Doesn't matter", UpdateSourceTrigger.Default);
            var u    = info.Create(null);
            Assert.Equal(BindingFeatures.None, info.Features);
            Assert.Null(u.PropertyType);

            var a = u.PropertySetValue(1);
            Assert.Equal(UpdateSourceResult.NotSet, a);
        }

        [Fact]
        public void T02_Should_update_target()
        {
            var testing = new TestingTool(_testOutputHelper);
            testing.bm.PropertyInfoProviderRegistry = new SimplePropertyInfoProviderRegistry(true);

            var dummy = new Dummy();

            var info = testing.bm.PropertyInfoProviderRegistry.FindProvider(dummy.GetType(), nameof(dummy.Name), UpdateSourceTrigger.Default);
            var u    = info.Create(dummy);
            Assert.Equal(BindingFeatures.None, info.Features);
            Assert.Equal(typeof(string), u.PropertyType);

            var a = u.PropertySetValue("1");
            Assert.Equal(UpdateSourceResultStatus.Ok, a.Status);
            Assert.Equal("1", dummy.Name);
        }

        [Theory]
        [InlineData(BindingMode.OneWay)]
        [InlineData(BindingMode.TwoWay)]
        public void T03_Should_update_target_when_connected(BindingMode mode)
        {
            var testing = new TestingTool(_testOutputHelper);
            testing.bm.PropertyInfoProviderRegistry = new SimplePropertyInfoProviderRegistry(true);

            var data   = new SimpleNpc(testing.log, "Obj") {Title = "Starting value"};
            var target = new Dummy();

            testing.bm
                .From(data, a => a.Title)
                .WithMode(mode)
                .BindTo(target, a => a.Name);
            Assert.Equal("Starting value", target.Name);
        }

        [Theory]
        [InlineData(BindingMode.OneWay)]
        [InlineData(BindingMode.TwoWay)]
        public void T04_Should_update_target_with(BindingMode mode)
        {
            var testing = new TestingTool(_testOutputHelper);
            testing.bm.PropertyInfoProviderRegistry = new SimplePropertyInfoProviderRegistry(true);

            var data   = new SimpleNpc(testing.log, "Obj") {Title = "Starting value"};
            var target = new Dummy();

            testing.bm
                .From(data, a => a.Title)
                .WithMode(mode)
                .BindTo(target, a => a.Name);
            Assert.Equal("Starting value", target.Name);

            data.Title = "Other title";
            Assert.Equal("Other title", target.Name);
        }

        [Theory(DisplayName = "T05 Should update source when target was changed but only for TWO-WAY scenario")]
        [InlineData(BindingMode.OneWay, "Starting value")]
        [InlineData(BindingMode.TwoWay, "New value")]
        public void T05_Should_update_source_with(BindingMode mode, string expected)
        {
            var testing = new TestingTool(_testOutputHelper);
            testing.bm.PropertyInfoProviderRegistry = new SimplePropertyInfoProviderRegistry(true);

            var data   = new SimpleNpc(testing.log, "Obj") {Title = "Starting value"};
            var target = new SimpleNpc(testing.log, "Target");

            var disposable = testing.bm
                .From(data, a => a.Title)
                .WithMode(mode)
                .BindTo(target, a => a.Title);
            Assert.Equal("Starting value", target.Title);
            target.Title = "New value";
            Assert.Equal(expected, data.Title);
            disposable.Dispose();
        }

        private readonly ITestOutputHelper _testOutputHelper;

        public class Dummy
        {
            public string Name { get; set; }
        }
    }
}
