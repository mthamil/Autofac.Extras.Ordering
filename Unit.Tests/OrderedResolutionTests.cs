using System.Linq;
using Autofac;
using Autofac.Extras.Ordering;
using Autofac.Features.Metadata;
using Xunit;

namespace Unit.Tests
{
    public class OrderedResolutionTests
    {
        [Fact]
        public void Test_ResolveOrdered()
        {
            // Arrange.
            _builder.Register(_ => new Dependency("dep 2")).As<IDependency>()
                    .OrderBy(d => d.Name);
            _builder.Register(_ => new OtherDependency("dep 3")).As<IDependency>()
                    .OrderBy(d => d.Name);
            _builder.Register(_ => new Dependency("dep 1")).As<IDependency>()
                    .OrderBy(d => d.Name);
            var container = _builder.Build();

            // Act.
            var dependencies = container.ResolveOrdered<IDependency>();

            // Assert.
            Assert.Equal(new[] { "dep 1", "dep 2", "dep 3" }, dependencies.Select(d => d.Name));
        }

        [Fact]
        public void Test_ResolveOrdered_With_Parameters()
        {
            // Arrange.
            _builder.RegisterType<Dependency>().As<IDependency>()
                    .OrderBy(2);
            _builder.RegisterType<OtherDependency>().As<IDependency>()
                    .OrderBy(1);
            _builder.RegisterType<YetAnotherDependency>().As<IDependency>()
                    .OrderBy(3);
            var container = _builder.Build();

            // Act.
            var dependencies = container.ResolveOrdered<IDependency>(TypedParameter.From("test"));

            // Assert.
            Assert.Equal(new[] { typeof(OtherDependency), typeof(Dependency), typeof(YetAnotherDependency) }, 
                         dependencies.Select(d => d.GetType()));
            Assert.Equal("test", dependencies.Select(d => d.Name).Distinct().Single());
        }

        [Fact]
        public void Test_ResolveOrdered_With_Additional_Metadata()
        {
            // Arrange.
            _builder.Register(_ => new Dependency("dep 2")).As<IDependency>()
                    .OrderBy(x => x.Name)
                    .WithMetadata<AdditionalMetadata>(m => m.For(p => p.Data, 1));
            _builder.Register(_ => new OtherDependency("dep 3")).As<IDependency>()
                    .OrderBy(x => x.Name)
                    .WithMetadata<AdditionalMetadata>(m => m.For(p => p.Data, 2));
            _builder.Register(_ => new Dependency("dep 1")).As<IDependency>()
                    .OrderBy(x => x.Name)
                    .WithMetadata<AdditionalMetadata>(m => m.For(p => p.Data, 3));
            var container = _builder.Build();

            // Act.
            var dependencies = container.ResolveOrdered<Meta<IDependency, AdditionalMetadata>>();

            // Assert.
            Assert.Equal(new[] { "dep 1", "dep 2", "dep 3" }, dependencies.Select(d => d.Value.Name));
            Assert.Equal(new[] { 3, 1, 2 }, dependencies.Select(d => d.Metadata.Data));
        }

        [Fact]
        public void Test_ResolveOrdered_For_Scanned_Types()
        {
            // Arrange.
            _builder.RegisterTypes(new[]
                    {
                        typeof(YetAnotherDependency),
                        typeof(Dependency),
                        typeof(OtherDependency)
                    })
                    .As<IDependency>()
                    .OrderByRegistration();
            var container = _builder.Build();

            // Act.
            var dependencies = container.ResolveOrdered<IDependency>();

            // Assert.
            Assert.Equal(new[] { typeof(YetAnotherDependency), typeof(Dependency), typeof(OtherDependency) },
                         dependencies.Select(d => d.GetType()));
        }

        private readonly ContainerBuilder _builder = new ContainerBuilder();

        private class AdditionalMetadata
        {
            public int Data { get; set; }
        }
    }
}