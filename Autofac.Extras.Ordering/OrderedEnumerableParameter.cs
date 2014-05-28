using System;
using System.Linq;
using System.Reflection;
using Autofac.Core;
using Autofac.Extras.Ordering.Utilities;

namespace Autofac.Extras.Ordering
{
    /// <summary>
    /// Parameter that can resolve values of type <see cref="IOrderedEnumerable{TElement}"/>.
    /// </summary>
    [Obsolete("Use OrderedRegistrationSource.")]
    public class OrderedEnumerableParameter : ResolvedParameter
    {
        /// <summary>
        /// Initializes a new <see cref="OrderedEnumerableParameter"/>.
        /// </summary>
        public OrderedEnumerableParameter()
            : base((p, c) => p.ParameterType.IsInstanceOfGenericType(typeof(IOrderedEnumerable<>)),
                   (p, c) => ResolveMethod.MakeGenericMethod(p.ParameterType.GetGenericArguments().Single())
                                          .Invoke(null, new object[] { c, EmptyParameters }))
        {
        }

        private static readonly MethodInfo ResolveMethod =
            typeof(OrderedResolutionExtensions).GetMethod("ResolveOrdered",
                                                          BindingFlags.Public |
                                                          BindingFlags.Static);

        private static readonly Parameter[] EmptyParameters = new Parameter[0];
    }
}