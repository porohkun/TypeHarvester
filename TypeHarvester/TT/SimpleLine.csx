// ReSharper disable once CheckNamespace

/// <summary>
/// Представляет обычную строку шаблона без директив, блоков кода или встроенных выражений.
/// </summary>
internal record SimpleLine(string Line) : ITemplateLine;
