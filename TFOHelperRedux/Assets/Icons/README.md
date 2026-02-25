# 🎨 Работа с SVG и векторной графикой в TFOHelperRedux

## ✅ Поддерживаемые форматы

| Формат | Поддержка | Использование |
|--------|-----------|---------------|
| **SVG** | ✅ Через SharpVectors | `Assets/Icons/*.svg` |
| **XAML Geometry** | ✅ Нативно (рекомендуется) | `Themes/Icons.xaml` |
| **PNG/JPG/GIF/BMP** | ✅ Нативно | Любые папки |

---

## 📁 Способ 1: Векторные иконки в XAML (РЕКОМЕНДУЕТСЯ)

### Преимущества:
- ⚡ Высокая производительность (нативный WPF)
- 🎨 Легко менять цвет через `Fill`
- 📐 Масштабирование без потерь
- 🔧 Не нужны внешние библиотеки

### Как использовать:

**1. Добавьте иконку в `Themes/Icons.xaml`:**
```xml
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">

    <!-- Ваша иконка -->
    <Geometry x:Key="IconMyIcon">M10,20 C15,25 25,25 30,20 ... Z</Geometry>
    
</ResourceDictionary>
```

**2. Используйте в XAML:**
```xml
<Path Data="{StaticResource IconMyIcon}" 
      Fill="{DynamicResource PrimaryColor}" 
      Width="32" Height="32"/>
```

**3. Или через кнопку:**
```xml
<Button Style="{StaticResource IconButton}">
    <Path Data="{StaticResource IconAdd}" 
          Fill="{DynamicResource TextPrimary}" 
          Width="20" Height="20"/>
</Button>
```

### Где взять Geometry из SVG?

**Онлайн конвертеры:**
- https://svg2xaml.com/
- https://www.aaronhayman.com/tools/svg-to-xaml/
- https://kiki.to/blog/2016/04/12/tip-converting-svgs-to-xaml/

**Процесс:**
1. Откройте SVG файл в конвертере
2. Скопируйте результат (Path Data)
3. Вставьте в `Icons.xaml` как `Geometry`

---

## 🖼️ Способ 2: Прямое использование SVG файлов

### Настройка:

**1. Поместите SVG файл:**
```
TFOHelperRedux/
├── Assets/
│   └── Icons/
│       └── my-icon.svg
```

**2. Файл автоматически копируется при сборке** (настроено в `.csproj`)

### Использование в XAML:

```xml
<UserControl xmlns:svgc="http://sharpvectors.codeplex.com/">
    
    <!-- Простое отображение -->
    <svgc:SvgViewbox Source="Assets/Icons/my-icon.svg" 
                     Width="32" Height="32"/>
    
    <!-- С изменением цвета -->
    <svgc:SvgViewbox Source="Assets/Icons/my-icon.svg"
                     Width="32" Height="32"
                     FillColor="#FF0000"/>
    
</UserControl>
```

### Использование в коде (C#):

```csharp
using SharpVectors.Converters.Wpf;

// Загрузка SVG
var svgConverter = new SvgConverter();
var drawing = svgConverter.ConvertFromUri(new Uri("Assets/Icons/my-icon.svg"));

// Или через SvgViewbox
var viewbox = new SvgViewbox
{
    Source = new Uri("Assets/Icons/my-icon.svg", UriKind.Relative),
    Width = 32,
    Height = 32
};
```

---

## 🎯 Доступные иконки (Icons.xaml)

В проекте уже есть набор иконок:

| Иконка | Ключ | Пример |
|--------|------|--------|
| 🐟 Рыба | `IconFish` | `<Path Data="{StaticResource IconFish}"/>` |
| 🗺️ Карта | `IconMap` | `<Path Data="{StaticResource IconMap}"/>` |
| 🎣 Удочка | `IconFishingRod` | `<Path Data="{StaticResource IconFishingRod}"/>` |
| ⚙️ Настройки | `IconSettings` | `<Path Data="{StaticResource IconSettings}"/>` |
| 💾 Сохранить | `IconSave` | `<Path Data="{StaticResource IconSave}"/>` |
| ✏️ Редактировать | `IconEdit` | `<Path Data="{StaticResource IconEdit}"/>` |
| 🗑️ Удалить | `IconDelete` | `<Path Data="{StaticResource IconDelete}"/>` |
| ➕ Добавить | `IconAdd` | `<Path Data="{StaticResource IconAdd}"/>` |
| ❌ Закрыть | `IconClose` | `<Path Data="{StaticResource IconClose}"/>` |
| ✔️ OK | `IconCheck` | `<Path Data="{StaticResource IconCheck}"/>` |
| 🔍 Поиск | `IconSearch` | `<Path Data="{StaticResource IconSearch}"/>` |
| 📦 Экспорт | `IconExport` | `<Path Data="{StaticResource IconExport}"/>` |
| 📥 Импорт | `IconImport` | `<Path Data="{StaticResource IconImport}"/>` |
| 🎨 Палитра | `IconPalette` | `<Path Data="{StaticResource IconPalette}"/>` |
| 🌙 Тёмная тема | `IconDarkTheme` | `<Path Data="{StaticResource IconDarkTheme}"/>` |
| ☀️ Светлая тема | `IconLightTheme` | `<Path Data="{StaticResource IconLightTheme}"/>` |
| 🔄 Обновить | `IconRefresh` | `<Path Data="{StaticResource IconRefresh}"/>` |
| ℹ️ Инфо | `IconInfo` | `<Path Data="{StaticResource IconInfo}"/>` |
| ⚠️ Предупреждение | `IconWarning` | `<Path Data="{StaticResource IconWarning}"/>` |
| 🏷️ Тег | `IconTag` | `<Path Data="{StaticResource IconTag}"/>` |
| 📊 График | `IconChart` | `<Path Data="{StaticResource IconChart}"/>` |
| 🔗 Ссылка | `IconLink` | `<Path Data="{StaticResource IconLink}"/>` |
| 📌 Булавка | `IconPin` | `<Path Data="{StaticResource IconPin}"/>` |
| 👁️ Видимость | `IconVisibility` | `<Path Data="{StaticResource IconVisibility}"/>` |
| 👁️‍🗨️ Скрыть | `IconVisibilityOff` | `<Path Data="{StaticResource IconVisibilityOff}"/>` |
| 🔒 Замок | `IconLock` | `<Path Data="{StaticResource IconLock}"/>` |
| 🔓 Открыть | `IconUnlock` | `<Path Data="{StaticResource IconUnlock}"/>` |
| 📋 Список | `IconList` | `<Path Data="{StaticResource IconList}"/>` |
| ▤ Сетка | `IconGrid` | `<Path Data="{StaticResource IconGrid}"/>` |

---

## 📝 Примеры использования

### Кнопка с иконкой:
```xml
<Button Style="{StaticResource MaterialDesignFlatButton}"
        Command="{Binding AddCommand}">
    <StackPanel Orientation="Horizontal">
        <Path Data="{StaticResource IconAdd}" 
              Fill="{DynamicResource PrimaryColor}" 
              Width="20" Height="20"
              Margin="0,0,8,0"/>
        <TextBlock Text="Добавить"/>
    </StackPanel>
</Button>
```

### Иконка в меню:
```xml
<MenuItem Header="Рыбы">
    <MenuItem.Icon>
        <Path Data="{StaticResource IconFish}" 
              Fill="{DynamicResource TextPrimary}" 
              Width="16" Height="16"/>
    </MenuItem.Icon>
</MenuItem>
```

### ToggleButton с иконками темы:
```xml
<ToggleButton IsChecked="{Binding IsDarkTheme}">
    <ToggleButton.Style>
        <Style TargetType="ToggleButton">
            <Setter Property="Content">
                <Setter.Value>
                    <Path Data="{StaticResource IconLightTheme}" 
                          Fill="{DynamicResource TextPrimary}" 
                          Width="20" Height="20"/>
                </Setter.Value>
            </Setter>
            <Style.Triggers>
                <Trigger Property="IsChecked" Value="True">
                    <Setter Property="Content">
                        <Setter.Value>
                            <Path Data="{StaticResource IconDarkTheme}" 
                                  Fill="{DynamicResource TextPrimary}" 
                                  Width="20" Height="20"/>
                        </Setter.Value>
                    </Setter>
                </Trigger>
            </Style.Triggers>
        </Style>
    </ToggleButton.Style>
</ToggleButton>
```

---

## 🛠️ Добавление новой иконки

1. **Найдите SVG иконку** (например, на https://materialdesignicons.com/)
2. **Сконвертируйте в XAML** через онлайн-конвертер
3. **Добавьте в `Themes/Icons.xaml`:**
   ```xml
   <Geometry x:Key="IconMyNewIcon">M10,20 ... Z</Geometry>
   ```
4. **Используйте:**
   ```xml
   <Path Data="{StaticResource IconMyNewIcon}" .../>
   ```

---

## ⚠️ Важные замечания

1. **Производительность:** XAML Geometry работает быстрее, чем SVG файлы
2. **Цвет:** SVG через `SvgViewbox` сложнее перекрашивать, чем XAML Geometry
3. **Размер:** XAML Geometry увеличивает размер приложения меньше, чем SVG файлы
4. **SharpVectors:** Установлен как NuGet пакет (версия 1.8.5)

---

## 📦 NuGet пакеты

```xml
<PackageReference Include="SharpVectors" Version="1.8.5" />
```
