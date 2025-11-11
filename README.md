# TypeHarvester

<details open>
<summary>en-US</summary>

### Overview

**TypeHarvester** is an **incremental Roslyn source generator** that scans assemblies for types marked with specified attributes and generates a strongly-typed cache class named `TypesByAttributes`.

This cache provides several overloads of the `Get` method, allowing you to retrieve all types annotated with one or more attributes **without using reflection at runtime**.

---

### Why Use It?

TypeHarvester exists to **eliminate reflection-based type discovery** and the numerous issues that come with it:

- The target assembly may not be loaded into the `AppDomain` when reflection runs.  
- Version conflicts or isolation between different `AssemblyLoadContext`s.  
- Assemblies missing from `AppDomain.CurrentDomain.GetAssemblies()` enumeration.  
- `ReflectionTypeLoadException` due to inaccessible types.  
- Problems with dynamic or reflection-only assemblies.  
- Attributes inaccessible due to missing dependency assemblies.  
- Types trimmed by the .NET linker (Trimming in .NET Core+).

By collecting metadata **at compile time**, TypeHarvester prevents these **leaky abstractions** of reflection and provides a **fast, reliable, compile-time cache** of attributed types.

---

### Installation

In your project’s `.csproj`:

```xml
<ItemGroup>
  <PackageReference Include="TypeHarvester" Version="latest">
    <PrivateAssets>all</PrivateAssets>
    <IncludeAssets> analyzers;</IncludeAssets>
  </PackageReference>
  <AdditionalFiles Include="codegen.config.json" />
</ItemGroup>
```

Then, place a `codegen.config.json` file in your project root:

```json
{
	"CollectTypesWithAttributes": {
		"Attributes": [
			"TypeHarvester.Debug.Dependency.MyAttribute"
		],
		"NamespaceForGenerations": "TypeHarvester.Debug",
		"CollectToCache": false,
		"Partial": false
	}
}
```

### Configuration Explained

|Setting|Description|
|---|---|
|`Attributes`|List of attribute types to collect.|
|`NamespaceForGenerations`|Namespace for generated code. Must match your stub file’s namespace. If omitted, code is generated in the global namespace (not recommended).|
|`CollectToCache`|If `true`, collected data is stored in assembly metadata instead of generating code. Used for dependent projects.|
|`Partial`|If `true`, generated class and methods are marked as `partial` to support IntelliSense fixes.|

---

### Multi-Project Setup

- For **dependent projects** (where you just collect attributed types):  
    Set `"CollectToCache": true` and omit `NamespaceForGenerations`.
- For the **top-level project** (where code is generated):  
    Set `"CollectToCache": false` and specify `NamespaceForGenerations`.

This way, TypeHarvester collects attributed types from all projects and consolidates them into a single generated class in the top-level project.

---

### IntelliSense Desync Fix

If IntelliSense reports that the generated class does not exist (but build succeeds), apply this workaround:

1. In `codegen.config.json`, set `"Partial": true`.
2. Add a stub file manually:
   ```csharp
   namespace TypeHarvester.Debug;
   
   using System;
   using System.Collections.Generic;
   
   internal static partial class TypesByAttributes
   {
   	   internal static partial IEnumerable<Type> Get<TAttribute>();
   	   internal static partial IEnumerable<Type> Get<TAttribute1, TAttribute2>();
   	   internal static partial IEnumerable<Type> Get<TAttribute1, TAttribute2, TAttribute3>();
   	   internal static partial IEnumerable<Type> Get(params Type[] attributeTypes);
   }
   ```
</details>

<details>
<summary>ru-RU</summary>

### Общее описание

**TypeHarvester** — это **инкрементальный Roslyn-генератор исходного кода**, который находит все типы, помеченные указанными атрибутами, и генерирует класс-кэш `TypesByAttributes`.

Этот класс содержит несколько перегрузок метода `Get`, позволяющих получать все типы, аннотированные одним или несколькими атрибутами, **без использования рефлексии во время выполнения**.

---

### Зачем это нужно?

Проект создан для того, чтобы **избавиться от использования рефлексии для поиска типов** и всех сопутствующих проблем:

- Сборка с нужным типом может не быть загружена в `AppDomain` в момент вызова.
- Конфликты версий сборок или изоляция в разных `AssemblyLoadContext`.
- Отсутствие нужных сборок в `AppDomain.CurrentDomain.GetAssemblies()`.
- Ошибка `ReflectionTypeLoadException` при частичной недоступности типов.
- Проблемы с динамическими или reflection-only сборками.
- Недоступность атрибутов из-за не загруженных зависимых сборок.
- Удаление типов оптимизатором (Trimming) в .NET Core+.

TypeHarvester решает проблему **протекающих абстракций рефлексии**, собирая данные **на этапе компиляции** и создавая быстрый и надёжный кэш типов.

---

### Подключение

В `.csproj` проекта:

```xml
<ItemGroup>
  <PackageReference Include="TypeHarvester" Version="latest">
    <PrivateAssets>all</PrivateAssets>
    <IncludeAssets> analyzers;</IncludeAssets>
  </PackageReference>
  <AdditionalFiles Include="codegen.config.json" />
</ItemGroup>
```

Создайте в корне проекта файл `codegen.config.json`:

```json
{
	"CollectTypesWithAttributes": {
		"Attributes": [
			"TypeHarvester.Debug.Dependency.MyAttribute"
		],
		"NamespaceForGenerations": "TypeHarvester.Debug",
		"CollectToCache": false,
		"Partial": false
	}
}
```

### Пояснение параметров

|Параметр|Описание|
|---|---|
|`Attributes`|Список атрибутов, по которым будут собираться типы.|
|`NamespaceForGenerations`|Пространство имён для сгенерированного кода. Должно совпадать с пространством имён stub-файла. Если отсутствует — генерация идёт в глобальный неймспейс (не рекомендуется).|
|`CollectToCache`|Если `true`, данные сохраняются в метаданные сборки, а не генерируется код. Используется в зависимых проектах.|
|`Partial`|Если `true`, код будет сгенерирован как `partial` для устранения ошибок IntelliSense.|

---

### Многопроектная схема

- В **зависимых проектах**:  
    `"CollectToCache": true`, параметр `NamespaceForGenerations` можно опустить.
- В **основном (топ-левел) проекте**:  
    `"CollectToCache": false` и обязательно указать `NamespaceForGenerations`.

Таким образом, генератор соберёт типы из всех подключённых проектов и создаст единый класс-кэш в основном проекте.

---

### Исправление ошибки IntelliSense

Если IntelliSense жалуется на отсутствие класса (при этом сборка успешно компилируется):

1. Установите `"Partial": true` в конфиге.
2. Добавьте stub-файл:
   ```csharp
   namespace TypeHarvester.Debug;
   
   using System;
   using System.Collections.Generic;
   
   internal static partial class TypesByAttributes
   {
   	   internal static partial IEnumerable<Type> Get<TAttribute>();
   	   internal static partial IEnumerable<Type> Get<TAttribute1, TAttribute2>();
   	   internal static partial IEnumerable<Type> Get<TAttribute1, TAttribute2, TAttribute3>();
   	   internal static partial IEnumerable<Type> Get(params Type[] attributeTypes);
   }
   ```

</details>