using System.Linq;
using Autofac;
using Autofac.Extras.Ordering;
using Xunit;

namespace Unit.Tests
{
    public class OrderedResolutionTests
    {
        [Fact]
        public void Test_ResolveOrdered()
        {
            // Arrange.
            _builder.Register(_ => new Dependency("dep 3")).As<IDependency>()
                    .OrderBy(d => d.Name);
            _builder.Register(_ => new OtherDependency("dep 2")).As<IDependency>()
                    .OrderBy(d => d.Name);
            _builder.Register(_ => new Dependency("dep 1")).As<IDependency>()
                    .OrderBy(d => d.Name);
            var container = _builder.Build();

            // Act.
            var dependencies = container.ResolveOrdered<IDependency>();

            // Assert.
            Assert.Equal(new[] { "dep 1", "dep 2", "dep 3" }, dependencies.Select(d => d.Name));
        }

        private readonly ContainerBuilder _builder = new ContainerBuilder();
    }
}