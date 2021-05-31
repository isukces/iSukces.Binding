using iSukces.Binding.Test.Data;
using Xunit;
using Xunit.Abstractions;

namespace iSukces.Binding.Test
{
    public class PropertyInfoProviderRegistryTests
    {
        public PropertyInfoProviderRegistryTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact(DisplayName = "01 Should_find_property_accessor_for_null")]
        public void T01_Should_find_property_accessor_for_null()
        {
            var testing = new TestingTool(_testOutputHelper);
            testing.bm.PropertyInfoProviderRegistry = new SimplePropertyInfoProviderRegistry(true);

            var info = testing.bm.PropertyInfoProviderRegistry.FindProvider(null, "Doesn't matter", UpdateSourceTrigger.Default);
            Assert.NotNull(info);
            Assert.Equal(BindingFeatures.None, info.Features);
        }

        [Fact(DisplayName = "02 Should find property accessor for not existing property")]
        public void T02_Should_find_property_accessor_for_not_existing_property()
        {
            var testing = new TestingTool(_testOutputHelper);
            testing.bm.PropertyInfoProviderRegistry = new SimplePropertyInfoProviderRegistry(true);

            var info = testing.bm.PropertyInfoProviderRegistry.FindProvider(typeof(Dummy), "ThisPropertyDoesntExist", UpdateSourceTrigger.Default);
            Assert.NotNull(info);
            Assert.Equal(BindingFeatures.None, info.Features);
        }

        [Fact(DisplayName = "03 Should find property accessor")]
        public void T03_Should_find_property_accessor()
        {
            var testing = new TestingTool(_testOutputHelper);
            testing.bm.PropertyInfoProviderRegistry = new SimplePropertyInfoProviderRegistry(true);

            var info = testing.bm.PropertyInfoProviderRegistry.FindProvider(typeof(Dummy), nameof(Dummy.Name), UpdateSourceTrigger.Default);
            Assert.NotNull(info);
            Assert.Equal(BindingFeatures.None, info.Features);
        }

        [Fact(DisplayName = "04 Should find property accessor")]
        public void T04_Should_find_property_accessor()
        {
            var testing = new TestingTool(_testOutputHelper);
            testing.bm.PropertyInfoProviderRegistry = new SimplePropertyInfoProviderRegistry(true);

            var info = testing.bm.PropertyInfoProviderRegistry.FindProvider(typeof(SimpleNpc), nameof(SimpleNpc.Name), UpdateSourceTrigger.Default);
            Assert.NotNull(info);
            Assert.Equal(BindingFeatures.OnPropertyChanged, info.Features);
        }


        private readonly ITestOutputHelper _testOutputHelper;

        public class Dummy
        {
            public string Name { get; set; }
        }
    }
}
