// ReSharper disable once CheckNamespace

using System.Text.RegularExpressions;

/// <summary>
/// Представляет строку с T4-директивой вида <c>&lt;#@ ... #&gt;</c>.
/// </summary>
internal record Directive : ITemplateLine
{
    private static readonly Regex ParameterRegex = new(
        @"(?<key>\w+)\s*=\s*""(?<value>[^""]*)""",
        RegexOptions.Compiled);

    private readonly Dictionary<string, string> _parameters;

    /// <summary>
    /// Инициализирует новый экземпляр <see cref="Directive"/> на основе совпадения регулярного выражения.
    /// </summary>
    /// <param name="match">Результат совпадения с <see cref="TemplateLineDetector"/>.</param>
    internal Directive(Match match)
    {
        Name = match.Groups["name"].Value;
        var attrs = match.Groups["attrs"].Value;

        _parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (Match paramMatch in ParameterRegex.Matches(attrs))
            _parameters[paramMatch.Groups["key"].Value] = paramMatch.Groups["value"].Value;
    }

    /// <summary>
    /// Имя директивы, например <c>template</c>, <c>output</c> или <c>parameter</c>.
    /// </summary>
    internal string Name { get; }

    /// <summary>
    /// Получает значение параметра по имени.
    /// </summary>
    /// <param name="paramName">Имя параметра.</param>
    /// <returns>Значение параметра.</returns>
    internal string this[string paramName] => _parameters[paramName];
}
