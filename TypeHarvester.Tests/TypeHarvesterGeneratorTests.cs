namespace TypeHarvester.Tests;

using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

public class TypeHarvesterGeneratorTests
{
    [Fact]
    public void TestGenerator()
    {
        // Тестовый код
        string testCode = """
                              namespace Test
                              {
                                  public class TestClass { }
                              }
                              """;

        var compilation = CSharpCompilation.Create("TestAssembly")
            .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
            .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
            .AddSyntaxTrees(CSharpSyntaxTree.ParseText(testCode));

        var generator = Activator.CreateInstance(AppDomain.CurrentDomain
            .GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Single(t => t.Name == "TypeHarvesterGenerator")) as IIncrementalGenerator;

        Assert.NotNull(generator);

        CSharpGeneratorDriver.Create(generator)
            .RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out _);

        // Проверяем, что генератор сработал
        Assert.True(outputCompilation.SyntaxTrees.Count() > 1);
    }
}
