// ReSharper disable once CheckNamespace

using System.Text.RegularExpressions;

/// <summary>
/// Представляет строку с встроенным выражением вида <c>&lt;#= ... #&gt;</c>.
/// </summary>
internal record InlineExpression : ITemplateLine
{
    /// <summary>
    /// Инициализирует новый экземпляр <see cref="InlineExpression"/> на основе совпадения регулярного выражения.
    /// </summary>
    /// <param name="match">Результат совпадения с <see cref="TemplateLineDetector"/>.</param>
    internal InlineExpression(Match match)
    {
        Before = match.Groups["before"].Value;
        Expression = match.Groups["expr"].Value;
        After = match.Groups["after"].Value;
    }

    /// <summary>
    /// Текст перед встроенным выражением.
    /// </summary>
    internal string Before { get; }

    /// <summary>
    /// Само выражение, которое будет вычислено при генерации.
    /// </summary>
    internal string Expression { get; }

    /// <summary>
    /// Текст после встроенного выражения.
    /// </summary>
    internal string After { get; }
}
