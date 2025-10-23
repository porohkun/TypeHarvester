// ReSharper disable once CheckNamespace

using System.Text.RegularExpressions;

/// <summary>
/// Представляет строку блока кода вида <c>&lt;# foreach (...) { #&gt;</c>.
/// </summary>
internal record CodeBlockLine : ITemplateLine
{
    /// <summary>
    /// Инициализирует новый экземпляр <see cref="CodeBlockLine"/> на основе совпадения регулярного выражения.
    /// </summary>
    /// <param name="match">Результат совпадения с <see cref="TemplateLineDetector"/>.</param>
    internal CodeBlockLine(Match match)
    {
        CodeBlock = match.Groups["code"].Value.Trim();
    }

    /// <summary>
    /// Текст блока кода, который будет вставлен в генерируемый файл.
    /// </summary>
    internal string CodeBlock { get; }
}
