namespace TypeHarvester.Converters;

using System;
using Abstractions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TypeHarvester;

/// <summary>
/// Конвертер для чтения отдельного конфига из общего файла
/// </summary>
/// <typeparam name="TConfig">Тип конфига</typeparam>
internal class ConfigConverter<TConfig> : JsonConverter<ConfigParseResult<TConfig>>
    where TConfig : BaseConfig
{
    /// <inheritdoc/>
    public override ConfigParseResult<TConfig> ReadJson(JsonReader reader, Type objectType, ConfigParseResult<TConfig>? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        try
        {
            var root = JObject.Load(reader)
                       ?? throw new JsonSerializationException("Ожидается объект");

            var configTypeName = typeof(TConfig).Name;
            var configPropertyName = configTypeName.Substring(0, configTypeName.Length - "Config".Length);
            var property = root.Properties().SingleOrDefault(p => p.Name == configPropertyName);
            return property == null
                ? new ConfigParseResult<TConfig>()
                : new ConfigParseResult<TConfig> { Config = property.Value.ToObject<TConfig>(serializer) }
                  ?? throw new JsonSerializationException($"Не удалось прочитать конфиг {configPropertyName}");
        }
        catch (Exception e)
        {
            return new ConfigParseResult<TConfig>()
            {
                Exception = e
            };
        }
    }

    /// <inheritdoc/>
    public override void WriteJson(JsonWriter writer, ConfigParseResult<TConfig>? value, JsonSerializer serializer)
        => throw new NotImplementedException("Сериализация не поддерживается.");
}
