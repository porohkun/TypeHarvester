namespace TypeHarvester;

using System;
using Abstractions;

/// <summary>
/// Результат чтения конфига + ошибка чтения
/// </summary>
/// <typeparam name="T">Тип конфига</typeparam>
internal record ConfigParseResult<T>
    where T : BaseConfig
{
    /// <summary>
    /// Конфиг
    /// </summary>
    internal T? Config { get; init; }

    /// <summary>
    /// Исключение
    /// </summary>
    internal Exception? Exception { get; init; }

    /// <summary>
    /// Конфиг включен
    /// </summary>
    internal bool IsEnabled => Config?.Enabled ?? false;

    /// <summary>
    /// Конфиг загружен
    /// </summary>
    internal bool IsLoaded => Config != null;
}
