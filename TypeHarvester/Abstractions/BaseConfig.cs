namespace TypeHarvester.Abstractions;

using Newtonsoft.Json;

/// <summary>
/// Базовый тип для конфига генератора
/// </summary>
internal abstract record BaseConfig
{
    /// <summary>
    /// Конфиг включен
    /// </summary>
    [JsonProperty]
    internal bool Enabled { get; init; } = true;
}
