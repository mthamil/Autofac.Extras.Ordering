using System;

namespace Autofac.Extras.Ordering.Utilities
{
    /// <summary>
    /// Contains extension methods for <see cref="Type"/>.
    /// </summary>
    internal static class TypeExtensions
    {
        /// <summary>
        /// Determines whether a type is a closed instance of a generic type definition.
        /// </summary>
        public static bool IsInstanceOfGenericType(this Type type, Type genericTypeDefinition)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == genericTypeDefinition;
        }
    }
}