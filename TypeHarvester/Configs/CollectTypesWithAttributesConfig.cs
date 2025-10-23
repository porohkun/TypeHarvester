namespace TypeHarvester.Configs;

using System.Collections.Immutable;
using Abstractions;
using Generators;
using Newtonsoft.Json;

/// <summary>
/// Конфиг для <see cref="CollectTypesWithAttributesGenerator"/>
/// </summary>
internal record CollectTypesWithAttributesConfig : BaseConfig
{
    /// <summary>
    /// Участвующие в генерации атрибуты
    /// </summary>
    [JsonProperty]
    internal ImmutableArray<string> Attributes { get; init; }

    /// <summary>
    /// True - сохраняет собранные данные в кэш компиляции
    /// False - генерит код на основе собранных данных и кэша
    /// </summary>
    [JsonProperty]
    internal bool CollectToCache { get; init; }

    /// <summary>
    /// Неймспейс для сгененрированного кода
    /// </summary>
    [JsonProperty]
    internal string? NamespaceForGenerations { get; init; }

    /// <summary>
    /// Если true - сгенеренный код будет partial и потребует stub файл
    /// </summary>
    [JsonProperty]
    internal bool Partial { get; init; }
}
