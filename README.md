Autofac.Extras.Ordering
=======================
An Autofac extension that adds recognition of IOrderedEnumerable&lt;T&gt; as a relationship type, allowing multiple dependencies to be resolved in a guaranteed order. 
By default, order is not guaranteed when resolving IEnumerable&lt;T&gt;.

Download
========
Visit https://www.nuget.org/packages/Autofac.Extras.Ordering/ to download.

Usage
=====
First, declare a constructor argument of type IOrderedEnumerable&lt;TDependency&gt; for the component dependent on multiple services:

```C#
    public class SomeComponent
    {
        public SomeComponent(IOrderedEnumerable<Dependency> dependencies)
        {
            Dependencies = dependencies;
        }

        public IEnumerable<Dependency> Dependencies { get; private set; }
    }
    
    public class Dependency
    {
        public Dependency(string name)
        {
            Name = name;
        }
        
        public string Name { get; private set; }
    }
```

Then, register that component as usual:

```C#
    builder.RegisterType<SomeComponent>();
```

Next, register dependencies with the order in which they should be provided using the extension method .OrderBy().

A constant order may be used:
```C#
    builder.Register(_ => new Dependency("1"))
           .OrderBy(1);

    builder.Register(_ => new Dependency("2"))
           .OrderBy(2);
```

Or, a function may be provided that determines the order based on a dependency's own properties:
```C#
    builder.Register(_ => new Dependency("1"))
           .OrderBy(d => d.Name);

    builder.Register(_ => new Dependency("2"))
           .OrderBy(d => d.Name);
```

Finally, add the OrderedRegistrationSource to enable support for IOrderedEnumerable&lt;T&gt;:
```C#
    builder.RegisterSource(new OrderedRegistrationSource());
```

When SomeComponent is resolved, it will be supplied with Dependencies sorted by the order each was given.