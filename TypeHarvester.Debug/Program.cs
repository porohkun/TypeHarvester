namespace TypeHarvester.Debug;

using System;
using Dependency;

static class Program
{
    static void Main(string[] args)
    {
        var types = TypesByAttributes.Get([typeof(MyAttribute)]);


        foreach (var type in types)
            Console.WriteLine(type.FullName!);
    }
}