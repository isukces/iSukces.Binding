using iSukces.Binding.Test.Data;

namespace iSukces.Binding.Test
{
    public class ReflectionAccessorTests
    {
        [Fact]
        public void T01_Should_read_property_value()
        {
            var obj = new Poco();
            var ra  = new PropertyAccessorMaker.ReflectionAccessor(obj);
            var q   = ra[nameof(obj.Name)];
            Assert.Null(q);
            obj.Name = "Piotr";
            q        = (string)ra[nameof(obj.Name)];
            Assert.Equal("Piotr", q);
        }
    }
}
