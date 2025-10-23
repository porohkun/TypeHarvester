// ReSharper disable once CheckNamespace

using System.Text;

/// <summary>
/// Динамический T4-генератор, позволяющий компилировать и выполнять шаблоны из ресурсов.
/// </summary>
public static class TtGenerator
{
    /// <summary>
    /// Генерирует код C# класса-генератора для шаблона.
    /// </summary>
    public static string GenerateCode(string templateText, string className)
    {
        var lines = templateText.Replace("\r\n", "\n").Split('\n');
        var sb = new StringBuilder();

        sb.AppendLine("namespace TypeHarvester.Generators;");
        sb.AppendLine();
        sb.AppendLine("using System;");
        sb.AppendLine("using System.Text;");
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine("using System.Linq;");
        sb.AppendLine();
        sb.AppendLine($"internal partial class {className}");
        sb.AppendLine("{");
        sb.AppendLine("    private string Generate(Dictionary<string, object> templateArguments)");
        sb.AppendLine("    {");
        sb.AppendLine("        var sb = new StringBuilder();");

        var parsedLines = lines.Select(TemplateLineDetector.Detect).ToArray();

        foreach (var line in parsedLines)
        {
            sb.AppendLine(line switch
            {
                Directive { Name: "parameter" } d => $"        var {d["name"]} = ({d["type"]})templateArguments[\"{d["name"]}\"];",
                CodeBlockLine b => $"        {b.CodeBlock}",
                InlineExpression e => $"        sb.AppendLine($\"{Escape(e.Before, true)}{{{e.Expression}}}{Escape(e.After, true)}\");",
                SimpleLine l => $"        sb.AppendLine(\"{Escape(l.Line)}\");",
                _ => string.Empty
            });
        }

        sb.AppendLine("        return sb.ToString();");
        sb.AppendLine("    }");
        sb.AppendLine("}");

        return sb.ToString();
    }

    private static string Escape(string line, bool asTemplate = false)
    {
        line = line.Replace("\\", @"\\");
        line = line.Replace("\"", "\\\"");
        if (asTemplate)
        {
            line = line.Replace("{", "{{");
            line = line.Replace("}", "}}");
        }

        return line;
    }
}
