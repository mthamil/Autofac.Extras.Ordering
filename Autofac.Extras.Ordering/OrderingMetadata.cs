using System;

namespace Autofac.Extras.Ordering
{
    /// <summary>
    /// Metadata containing a function used to select a value that determines a service's resolution order.
    /// </summary>
    public class OrderingMetadata<TService>
    {
        /// <summary>
        /// A function used to select a value that determines a service's resolution order.
        /// </summary>
        public Func<TService, IComparable> KeySelector { get; set; } 
    }
}