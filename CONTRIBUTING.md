# Contributing to TypeHarvester

<details open>
<summary>en-US</summary>

### Development Setup

TypeHarvester consists of several projects, each serving a specific purpose:

| Project | Purpose |
|---------|---------|
| `CodeGeneratorHost` | Host project to provide IntelliSense for `.csx` generator scripts. Can be unloaded in regular development. |
| `TypeHarvester` | Core analyzer and source generator project. |
| `TypeHarvester.Debug` | Debug host project simulating a real use case for generator debugging. |
| `TypeHarvester.Debug.Dependency` | Auxiliary project to test multi-project scenarios. |
| `TypeHarvester.Tests` | Unit tests. |

---

### Building and Debugging

To attach a debugger to the analyzer:

1. Select the **DebugCompilation** profile.  
2. Build the `TypeHarvester.Debug` project.  
3. The debugger will automatically attach during generator initialization in build time.

</details>

<details>
<summary>ru-RU</summary>

### Настройка разработки

TypeHarvester состоит из нескольких проектов, каждый из которых имеет своё назначение:

| Проект | Назначение |
|--------|------------|
| `CodeGeneratorHost` | Проект-хост для подключения IntelliSense к `.csx` скриптам генератора. В обычной разработке можно выгружать. |
| `TypeHarvester` | Основной проект анализатора и генератора кода. |
| `TypeHarvester.Debug` | Проект для отладки генератора, имитирующий работу реального проекта. |
| `TypeHarvester.Debug.Dependency` | Вспомогательный проект для тестирования многопроектных сценариев. |
| `TypeHarvester.Tests` | Юнит тесты. |

---

### Сборка и отладка

Чтобы подключить отладчик к анализатору:

1. Выберите профиль **DebugCompilation**.  
2. Соберите проект `TypeHarvester.Debug`.  
3. Отладчик автоматически подключится в начале инициализации генератора во время сборки.

</details>