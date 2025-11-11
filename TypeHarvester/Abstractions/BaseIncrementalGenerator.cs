namespace TypeHarvester.Abstractions;

using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Converters;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Newtonsoft.Json;
using TypeHarvester;

/// <summary>
/// Базовый класс для инкрементальных кодогенераторов Roslyn.
/// Обеспечивает общую инфраструктуру для чтения конфигурации, фильтрации синтаксических узлов
/// и генерации исходных файлов.
/// </summary>
/// <typeparam name="TGenerator">Тип конкретного генератора, наследующего данный класс.</typeparam>
/// <typeparam name="TConfig">Тип конфигурационного объекта, управляющего генерацией.</typeparam>
/// <typeparam name="TData">Тип данных, извлекаемых из синтаксического дерева для генерации.</typeparam>
internal abstract class BaseIncrementalGenerator<TGenerator, TConfig, TData> : IIncrementalGenerator
    where TGenerator : BaseIncrementalGenerator<TGenerator, TConfig, TData>
    where TConfig : BaseConfig
{
    /// <summary>
    /// Инициализирует генератор и определяет его этапы обработки:
    /// чтение конфигурации, фильтрацию, трансформацию и генерацию исходных файлов.
    /// </summary>
    /// <param name="context">Контекст инициализации генератора Roslyn.</param>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
#if ATTACH_DEBUGGER && DEBUG
        if (!System.Diagnostics.Debugger.IsAttached)
        {
            System.Diagnostics.Debugger.Launch();
        }
#endif
        var jsonFiles = context.AdditionalTextsProvider
            .Where(f => Path.GetFileName(f.Path).Contains("codegen.config.json"));

        var configData = jsonFiles.Select((file, cancellationToken) =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                var text = file.GetText(cancellationToken)?.ToString() ?? string.Empty;
                return JsonConvert.DeserializeObject<ConfigParseResult<TConfig>>(text, new ConfigConverter<TConfig>())!;
            }
            catch (Exception e)
            {
                return new ConfigParseResult<TConfig>
                {
                    Exception = e
                };
            }
        });

        var combined = configData
            .Combine(context.CompilationProvider)
            .Combine(context.SyntaxProvider.CreateSyntaxProvider(
                    predicate: NodeFilterWithCancellationTokenHandling,
                    transform: ContextTransformWithCancellationTokenHandling)
                .Where(x => x is not null)
                .Select((x, _) => x!)
                .Collect());

        var filtered = combined
            .Where(x => x.Left.Left.IsEnabled || !x.Left.Left.IsLoaded)
            .Select((x, _) => x.Left.Left.IsLoaded
                ? x with
                {
                    Right = NeedsFiltering(x.Left.Left.Config!)
                        ? x.Right.Where(r => FilterByConfig(r, x.Left.Left.Config!)).ToImmutableArray()
                        : x.Right
                }
                : x with { Right = ImmutableArray<TData>.Empty });

        context.RegisterSourceOutput(
            filtered,
            (spc, triple) =>
            {
                var (tuple, syntaxData) = triple;
                var (config, compilation) = tuple;

                var toCache = IsCollectToCache(config.Config!);
                switch (config)
                {
                    case { IsEnabled: true, IsLoaded: true } when toCache:
                        spc.AddSource(
                            "__CacheStore.g.cs",
                            GenerateCacheFile(compilation.Assembly.Name, syntaxData.Select(CacheSyntaxEntry)
                                .ToArray()));
                        break;
                    case { IsEnabled: true, IsLoaded: true } when !toCache:
                        GenerateFilesWithExceptionHandling(spc, config.Config!, ExtractAllCacheData(compilation)
                            .Concat(syntaxData)
                            .ToArray());
                        break;
                    case { IsEnabled: false, IsLoaded: true }:
                        ClearFilesWithExceptionHandling(spc);
                        break;
                    case { IsLoaded: false }:
                        ReportException(
                            spc,
                            "CPE0001",
                            $"Ошибка чтения конфига для {nameof(TConfig)}",
                            config.Exception,
                            Location.Create("codegen.config.json", TextSpan.FromBounds(0, 0), default));
                        break;
                }
            });
    }

    protected abstract bool IsCollectToCache(TConfig config);

    protected abstract string CacheSyntaxEntry(TData entry);

    protected abstract TData SyntaxEntryFromCache(string cache);

    /// <summary>
    /// Определяет фильтр узлов синтаксического дерева, по которым будет выполняться анализ.
    /// </summary>
    /// <param name="node">Проверяемый узел синтаксического дерева.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns><see langword="true"/>, если узел подходит для анализа; иначе <see langword="false"/>.</returns>
    protected abstract bool NodeFilter(SyntaxNode node, CancellationToken cancellationToken);

    /// <summary>
    /// Преобразует отфильтрованный узел синтаксического дерева в объект данных для дальнейшей генерации.
    /// </summary>
    /// <param name="context">Контекст генератора для текущего узла.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Результат трансформации или <see langword="null"/>, если узел следует пропустить.</returns>
    protected abstract TData? ContextTransform(GeneratorSyntaxContext context, CancellationToken cancellationToken);

    /// <summary>
    /// Определяет, требуется ли дополнительная фильтрация результатов анализа на основе настроек конфига.
    /// </summary>
    /// <param name="config">Текущий конфиг генератора.</param>
    /// <returns><see langword="true"/>, если фильтрация включена; иначе <see langword="false"/>.</returns>
    protected abstract bool NeedsFiltering(TConfig config);

    /// <summary>
    /// Проверяет, соответствует ли элемент результату фильтрации, определённой конфигом.
    /// </summary>
    /// <param name="item">Элемент, полученный из синтаксического анализа.</param>
    /// <param name="config">Текущий конфиг генератора.</param>
    /// <returns><see langword="true"/>, если элемент удовлетворяет фильтру; иначе <see langword="false"/>.</returns>
    protected abstract bool FilterByConfig(TData item, TConfig config);

    /// <summary>
    /// Выполняет генерацию исходных файлов на основе результатов анализа и текущего конфига.
    /// </summary>
    /// <param name="spc">Контекст генерации исходников.</param>
    /// <param name="config">Текущий конфиг генератора.</param>
    /// <param name="values">Отфильтрованные данные для генерации.</param>
    protected abstract void GenerateFiles(SourceProductionContext spc, TConfig config, TData[] values);

    /// <summary>
    /// Удаляет или очищает ранее сгенерированные файлы, если генерация отключена.
    /// </summary>
    /// <param name="spc">Контекст генерации исходников.</param>
    protected abstract void ClearFiles(SourceProductionContext spc);

    private string GenerateCacheFile(string projectName, IEnumerable<string> items)
    {
        var sb = new StringBuilder();

        sb.AppendLine("//------------------------------------------------------------------------------");
        sb.AppendLine("// <auto-generated>");
        sb.AppendLine("//     Этот код был создан автоматически.");
        sb.AppendLine("//     Внесение изменений в этот файл может привести к неправильной работе");
        sb.AppendLine("//     и будет потеряно при повторной генерации кода.");
        sb.AppendLine("// </auto-generated>");
        sb.AppendLine("//------------------------------------------------------------------------------");
        sb.AppendLine("");
        foreach (var item in items)
            sb.AppendLine($"[assembly: System.Reflection.AssemblyMetadataAttribute(\"{typeof(TGenerator).Name}\", \"{item}\")]");

        return sb.ToString();
    }

    private IEnumerable<TData> ExtractAllCacheData(Compilation compilation) =>
        compilation.SourceModule.ReferencedAssemblySymbols
            .SelectMany(asm => asm.GetAttributes()
                .Where(attr => attr.AttributeClass?.ToDisplayString() == "System.Reflection.AssemblyMetadataAttribute"
                               && attr.ConstructorArguments[0].Value is string genName
                               && genName == typeof(TGenerator).Name))
            .Select(attr => attr.ConstructorArguments[1].Value is string cacheData
                ? SyntaxEntryFromCache(cacheData)
                : default)
            .OfType<TData>();

    private bool NodeFilterWithCancellationTokenHandling(SyntaxNode node, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return NodeFilter(node, cancellationToken);
    }

    private TData? ContextTransformWithCancellationTokenHandling(GeneratorSyntaxContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return ContextTransform(context, cancellationToken);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ReportException(SourceProductionContext spc, string id, string message, Exception? exception, Location? location = null)
    {
        spc.ReportDiagnostic(
            Diagnostic.Create(
                new DiagnosticDescriptor(
                    id,
                    message,
                    exception != null ? $"{exception.Message}: {exception.StackTrace}" : "Неизвестная ошибка",
                    nameof(TGenerator),
                    DiagnosticSeverity.Error,
                    true),
                location ?? Location.None));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void GenerateFilesWithExceptionHandling(SourceProductionContext spc, TConfig config, TData[] values)
    {
        try
        {
            GenerateFiles(spc, config, values);
        }
        catch (Exception e)
        {
            ReportException(
                spc,
                "GGE0001",
                $"Ошибка генерации в {nameof(TGenerator)}",
                e);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ClearFilesWithExceptionHandling(SourceProductionContext spc)
    {
        try
        {
            ClearFiles(spc);
        }
        catch (Exception e)
        {
            ReportException(
                spc,
                "CGE0001",
                $"Ошибка очистки в {nameof(TGenerator)}",
                e);
        }
    }
}
