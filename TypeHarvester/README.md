# TypeHarvester

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

In your project�s `.csproj`:

```xml
<ItemGroup>
  <ProjectReference Include="TypeHarvester" Version="latest" />
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
|`NamespaceForGenerations`|Namespace for generated code. Must match your stub file�s namespace. If omitted, code is generated in the global namespace (not recommended).|
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

---
---

### ����� ��������

**TypeHarvester** � ��� **��������������� Roslyn-��������� ��������� ����**, ������� ������� ��� ����, ���������� ���������� ����������, � ���������� �����-��� `TypesByAttributes`.

���� ����� �������� ��������� ���������� ������ `Get`, ����������� �������� ��� ����, �������������� ����� ��� ����������� ����������, **��� ������������� ��������� �� ����� ����������**.

---

### ����� ��� �����?

������ ������ ��� ����, ����� **���������� �� ������������� ��������� ��� ������ �����** � ���� ������������� �������:

- ������ � ������ ����� ����� �� ���� ��������� � `AppDomain` � ������ ������.
- ��������� ������ ������ ��� �������� � ������ `AssemblyLoadContext`.
- ���������� ������ ������ � `AppDomain.CurrentDomain.GetAssemblies()`.
- ������ `ReflectionTypeLoadException` ��� ��������� ������������� �����.
- �������� � ������������� ��� reflection-only ��������.
- ������������� ��������� ��-�� �� ����������� ��������� ������.
- �������� ����� ������������� (Trimming) � .NET Core+.

TypeHarvester ������ �������� **����������� ���������� ���������**, ������� ������ **�� ����� ����������** � �������� ������� � ������� ��� �����.

---

### �����������

� `.csproj` �������:

```xml
<ItemGroup>
  <ProjectReference Include="TypeHarvester" Version="latest" />
  <AdditionalFiles Include="codegen.config.json" />
</ItemGroup>
```

�������� � ����� ������� ���� `codegen.config.json`:

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

### ��������� ����������

|��������|��������|
|---|---|
|`Attributes`|������ ���������, �� ������� ����� ���������� ����.|
|`NamespaceForGenerations`|������������ ��� ��� ���������������� ����. ������ ��������� � ������������� ��� stub-�����. ���� ����������� � ��������� ��� � ���������� ��������� (�� �������������).|
|`CollectToCache`|���� `true`, ������ ����������� � ���������� ������, � �� ������������ ���. ������������ � ��������� ��������.|
|`Partial`|���� `true`, ��� ����� ������������ ��� `partial` ��� ���������� ������ IntelliSense.|

---

### �������������� �����

- � **��������� ��������**:  
    `"CollectToCache": true`, �������� `NamespaceForGenerations` ����� ��������.
- � **�������� (���-�����) �������**:  
    `"CollectToCache": false` � ����������� ������� `NamespaceForGenerations`.

����� �������, ��������� ������ ���� �� ���� ������������ �������� � ������� ������ �����-��� � �������� �������.

---

### ����������� ������ IntelliSense

���� IntelliSense �������� �� ���������� ������ (��� ���� ������ ������� �������������):

1. ���������� `"Partial": true` � �������.
2. �������� stub-����:
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
1. 