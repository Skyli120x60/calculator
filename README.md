# 🧮 Calculator by fadeev

Настольный калькулятор на C# / WPF (.NET 8) с вычислением арифметических выражений, историей операций и поддержкой тёмной/светлой темы.

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=flat-square&logo=dotnet)](https://dotnet.microsoft.com/)
[![WPF](https://img.shields.io/badge/UI-WPF-0078D4?style=flat-square&logo=windows)](https://learn.microsoft.com/ru-ru/dotnet/desktop/wpf/)
[![C#](https://img.shields.io/badge/Language-C%23-239120?style=flat-square&logo=csharp)](https://learn.microsoft.com/ru-ru/dotnet/csharp/)
[![Tests](https://img.shields.io/badge/Tests-xUnit-green?style=flat-square)](https://xunit.net/)
[![License](https://img.shields.io/badge/License-MIT-blue?style=flat-square)](LICENSE)
[![Windows](https://img.shields.io/badge/OS-Windows-0078D4?style=flat-square&logo=windows)](https://www.microsoft.com/windows)
[![Release](https://img.shields.io/github/v/release/Linkimin/Calculator?style=flat-square&logo=github)](https://github.com/Linkimin/Calculator/releases)
[![Downloads](https://img.shields.io/github/downloads/Linkimin/Calculator/total?style=flat-square&logo=github)](https://github.com/Linkimin/Calculator/releases)

## ✨ Возможности

- Вычисление арифметических выражений: `+` `-` `*` `/` `^` и скобки
- Унарный минус (`-5+2`, `3*-2`)
- Десятичные числа — точка или запятая как разделитель
- Предпросмотр результата в реальном времени
- История вычислений с возможностью восстановить выражение
- Тёмная и светлая тема с сохранением настройки
- Локализация интерфейса RU / EN
- Ввод с клавиатуры: цифры, операторы, Enter, Backspace, Escape

## 🏗️ Архитектура
```
FadeevCalculator/
├── FadeevCalculatorLib/       # Библиотека классов (бизнес-логика)
│   ├── Calculators/              # Shunting-yard алгоритм, CalculatorEngine
│   ├── Models/                   # CalculationHistoryItem, ThemeSettings
│   ├── Services/                 # HistoryService, SettingsService (JSON)
│   └── Infrastructure/           # AppPaths
├── FadeevCalculatorApp/       # WPF-приложение
│   ├── ViewModels/               # MainViewModel (MVVM)
│   └── Infrastructure/           # RelayCommand, StringToBrushConverter
└── FadeevCalculatorTests/     # xUnit тесты (25+)
```

**Ключевые решения:**
- Алгоритм **Shunting-yard** для разбора и вычисления выражений
- Тип `decimal` для точных вычислений без floating-point ошибок
- **MVVM** — ViewModel не зависит от WPF (`Brush` заменён на HEX-строки + конвертер)
- Иммутабельная модель `CalculationHistoryItem` с `[JsonConstructor]`
- История и настройки персистируются в `%LocalAppData%\FadeevCalculator\`

## 🚀 Запуск

### Готовый билд
Скачай последний релиз со страницы [Releases](../../releases) и запусти `FadeevCalculatorApp.exe`.

### Из исходников
```bash
git clone https://github.com/Linkimin/Calculator.git
cd Calculator
dotnet build FadeevCalculator.sln -c Release
dotnet run --project FadeevCalculatorApp/FadeevCalculatorApp.csproj
```

Либо открой `FadeevCalculator.sln` в Visual Studio 2022 и запусти `FadeevCalculatorApp`.

## 🧪 Тесты
```bash
dotnet test FadeevCalculator.sln -c Release
```

Покрытие включает базовые операции, приоритет, унарный минус, граничные случаи степени, негативные кейсы и персистентность.

## 🛠️ Требования

- Windows 10 / 11
- .NET 8 SDK (для сборки из исходников)

## 👤 Автор

**Фадеев Ярослав** — РЭУ им. Плеханова, Пермский филиал
