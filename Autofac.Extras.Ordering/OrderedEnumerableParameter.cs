using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac.Core;
using Autofac.Features.Metadata;

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

        private static object ResolveOrderedEnumerable<TService>(IComponentContext context)
        {
            var registeredType = typeof(IEnumerable<>).MakeGenericType(
                                 typeof(Meta<>).MakeGenericType(typeof(TService)));
            var resolved = (Meta<TService>[])context.Resolve(registeredType);
            return new AlreadyOrderedEnumerable<TService>(
                resolved.OrderBy(GetOrderFromMetadata)
                        .Select(t => t.Value)
                        .ToArray());
        }

        private static object GetOrderFromMetadata<TService>(Meta<TService> instance)
        {
            var orderingFunction = instance.Metadata.Single(m => m.Key == OrderingMetadataKey);
            return ((Delegate)orderingFunction.Value).DynamicInvoke(instance.Value);
        }

        private static readonly MethodInfo ResolveMethod =
            typeof(OrderedEnumerableParameter).GetMethod("ResolveOrderedEnumerable",
                                                          BindingFlags.NonPublic |
                                                          BindingFlags.Static);

        internal const string OrderingMetadataKey = "AutofacOrderingMetadataKey";

        private class AlreadyOrderedEnumerable<T> : IOrderedEnumerable<T>
        {
            public AlreadyOrderedEnumerable(IEnumerable<T> wrapped)
            {
                _wrapped = wrapped;
            }

            /// <summary>
            /// This method would be invoked if OrderBy was called again on the collection (unlikely for this usage).
            /// </summary>
            public IOrderedEnumerable<T> CreateOrderedEnumerable<TKey>(Func<T, TKey> keySelector, IComparer<TKey> comparer, bool @descending)
            {
                return @descending
                    ? _wrapped.OrderByDescending(keySelector, comparer)
                    : _wrapped.OrderBy(keySelector, comparer);
            }

            public IEnumerator<T> GetEnumerator()
            {
                return _wrapped.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            private readonly IEnumerable<T> _wrapped;
        }
    }
}