# WPF Commander — Лабораторная работа №4

Двухпанельный файловый менеджер на C# / WPF / .NET 8 / MVVM.

## Запуск

```powershell
cd WpfCommander
dotnet restore
dotnet run
```

Либо открыть `WpfCommander.csproj` в Visual Studio 2022 / JetBrains Rider и нажать F5.

Требуется .NET 8 SDK и Windows (WPF — Windows-only).

## Управление

| Клавиша / действие         | Что делает                                   |
|----------------------------|----------------------------------------------|
| Двойной клик / **Enter**   | Войти в папку или подняться по «..»          |
| **Backspace**              | Вверх по дереву каталогов                    |
| **F5**                     | Копировать выделенное в пассивную панель     |
| **F6**                     | Переместить выделенное в пассивную панель    |
| **F7**                     | Создать новую папку в активной панели        |
| **F8**                     | Удалить выделенное (с подтверждением)        |
| Клик по панели             | Сделать её активной (источник операций)      |

Активная панель подсвечивается синей рамкой.

## Структура

```
WpfCommander/
├── App.xaml + App.xaml.cs
├── WpfCommander.csproj
│
├── Commands/
│   └── RelayCommand.cs
│
├── Models/
│   └── FileSystemItem.cs       (INotifyPropertyChanged)
│
├── Services/
│   └── FileManagerService.cs   (вся файловая логика)
│
├── ViewModels/
│   ├── ViewModelBase.cs
│   ├── PanelViewModel.cs       (одна панель)
│   └── MainViewModel.cs        (координирует две панели)
│
└── Views/
    ├── MainWindow.xaml + .cs
    ├── PanelView.xaml + .cs    (UserControl панели)
    └── InputDialog.xaml + .cs  (диалог имени папки)
```
