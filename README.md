Autofac.Extras.Ordering
=======================
An Autofac extension that adds recognition of IOrderedEnumerable&lt;T&gt; as a relationship type, allowing multiple dependencies to be resolved in a guaranteed order. 
By default, order is not guaranteed when resolving IEnumerable&lt;T&gt;.

Usage
=====
First, declare a constructor argument of type IOrderedEnumerable&lt;TDependency&gt; for the component dependent on multiple services:

```c#
    public class SomeComponent
    {
        public SomeComponent(IOrderedEnumerable<Dependency> dependencies)
        {
            Dependencies = dependencies;
        }

        public IEnumerable<Dependency> Dependencies { get; private set; }
    }
```

Then, register that component using the extension method, .UsingOrdering():

```c#
    builder.RegisterType<TestComponent>()
           .UsingOrdering();
```

Finally, register dependencies with the order in which they should be provided using the extension method .WithOrder(int):

```c#
    builder.Register(_ => new Dependency("1"))
           .WithOrder(1);

    builder.Register(_ => new Dependency("2"))
           .WithOrder(2);
```

When SomeComponent is resolved, it will be supplied with Dependencies sorted by the order each was given.