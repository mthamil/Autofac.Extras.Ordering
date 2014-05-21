using System.Linq;
using System.Reflection;
using Autofac.Core;

namespace Autofac.Extras.Ordering
{
    /// <summary>
    /// Parameter that can resolve values of type <see cref="IOrderedEnumerable{TElement}"/>.
    /// </summary>
    public class OrderedEnumerableParameter : ResolvedParameter
    {
        /// <summary>
        /// Initializes a new <see cref="OrderedEnumerableParameter"/>.
        /// </summary>
        public OrderedEnumerableParameter()
            : base((p, c) => p.ParameterType.IsGenericType && p.ParameterType.GetGenericTypeDefinition() == typeof(IOrderedEnumerable<>),
                   (p, c) => ResolveMethod.MakeGenericMethod(p.ParameterType.GetGenericArguments().Single())
                                          .Invoke(null, new object[] { c }))
        {
        }

        private static readonly MethodInfo ResolveMethod =
            typeof(OrderedResolutionExtensions).GetMethod("ResolveOrdered",
                                                          BindingFlags.Public |
                                                          BindingFlags.Static);

        internal const string OrderingMetadataKey = "AutofacOrderingMetadataKey";
    }
}