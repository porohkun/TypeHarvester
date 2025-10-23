namespace TypeHarvester.Debug;

using System;
using System.Collections.Generic;

internal static partial class TypesByAttributes
{
    internal static partial IEnumerable<Type> Get<TAttribute>();
    internal static partial IEnumerable<Type> Get<TAttribute1, TAttribute2>();
    internal static partial IEnumerable<Type> Get<TAttribute1, TAttribute2, TAttribute3>();
    internal static partial IEnumerable<Type> Get(params Type[] attributeTypes);
}
