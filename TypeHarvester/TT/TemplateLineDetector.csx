// ReSharper disable once CheckNamespace

using System.Text.RegularExpressions;

/// <summary>
/// Детектор типа строки шаблона T4.
/// Разбирает директивы, блоки кода, встроенные выражения и простые строки.
/// </summary>
internal static class TemplateLineDetector
{
    private static readonly Regex DirectiveRegex = new(
        @"^<#@\s*(?<name>\w+)\s+(?<attrs>.*?)\s*#>$",
        RegexOptions.Compiled | RegexOptions.Singleline);

    private static readonly Regex CodeBlockRegex = new(
        @"^<#\s*(?<code>.*?)\s*#>$",
        RegexOptions.Compiled);

    private static readonly Regex InlineExpressionRegex = new(
        @"^(?<before>.*?)<\#=\s*(?<expr>.*?)\s*#>(?<after>.*)$",
        RegexOptions.Compiled);

    /// <summary>
    /// Определяет тип строки шаблона и возвращает соответствующий объект.
    /// </summary>
    /// <param name="line">Одна строка шаблона.</param>
    /// <returns>Объект, реализующий <see cref="ITemplateLine"/>.</returns>
    internal static ITemplateLine Detect(string line)
    {
        Match match;

        if ((match = DirectiveRegex.Match(line)).Success)
            return new Directive(match);

        if ((match = CodeBlockRegex.Match(line.Trim())).Success)
            return new CodeBlockLine(match);

        if ((match = InlineExpressionRegex.Match(line)).Success)
            return new InlineExpression(match);

        return new SimpleLine(line);
    }
}
