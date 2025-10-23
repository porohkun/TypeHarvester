namespace TypeHarvester;

internal static class Extensions
{
    internal static string WithoutGlobal(this string typeName) =>
        typeName.StartsWith("global::") ? typeName.Substring(8) : typeName;
}
