using System.IO;
using System.Collections.ObjectModel;
using System.Windows;
using FileVault.Models;
using FileVault.Services;

namespace FileVault.ViewModels;

// VM одной панели
public class ExplorerPaneVM : ViewModelBase
{
    private readonly DiskService _disk;

    private string _currentPath = "";
    private string _selectedDrive = "";
    private FsEntry? _selectedEntry;
    private bool _isActive;

    public string CurrentPath
    {
        get => _currentPath;
        set { Set(ref _currentPath, value); OnPropertyChanged(nameof(Title)); }
    }

    public string SelectedDrive
    {
        get => _selectedDrive;
        set { if (Set(ref _selectedDrive, value)) NavigateTo(value); }
    }

    public FsEntry? SelectedEntry
    {
        get => _selectedEntry;
        set => Set(ref _selectedEntry, value);
    }

    // Подсветка активной панели
    public bool IsActive
    {
        get => _isActive;
        set { Set(ref _isActive, value); OnPropertyChanged(nameof(BorderColor)); }
    }

    public string BorderColor => _isActive ? "#4A9EFF" : "#2A2A3A";
    public string Title => CurrentPath;

    public ObservableCollection<string> Drives { get; } = new();
    public ObservableCollection<FsEntry> Entries { get; } = new();

    public ExplorerPaneVM(DiskService disk)
    {
        _disk = disk;
        LoadDrives();
    }

    // Загрузка списка дисков
    public void LoadDrives()
    {
        Drives.Clear();
        foreach (var d in _disk.GetDrives())
            Drives.Add(d);

        if (Drives.Count > 0)
        {
            _selectedDrive = Drives[0];
            OnPropertyChanged(nameof(SelectedDrive));
            NavigateTo(Drives[0]);
        }
    }

    // Навигация в директорию
    public void NavigateTo(string path)
    {
        try
        {
            var entries = _disk.GetEntries(path);
            Entries.Clear();
            foreach (var e in entries)
                Entries.Add(e);
            CurrentPath = path;

            // Синхронизируем ComboBox с буквой диска
            var root = Path.GetPathRoot(path) ?? path;
            if (_selectedDrive != root)
            {
                _selectedDrive = root;
                OnPropertyChanged(nameof(SelectedDrive));
            }
        }
        catch (UnauthorizedAccessException ex)
        {
            MessageBox.Show(ex.Message, "Доступ запрещён",
                MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Ошибка",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    // Двойной клик / Enter
    public void OpenSelected()
    {
        if (SelectedEntry == null) return;
        if (SelectedEntry.IsDirectory)
            NavigateTo(SelectedEntry.FullPath);
    }

    // Backspace — уровень выше
    public void GoUp()
    {
        var parent = Directory.GetParent(CurrentPath);
        if (parent != null)
            NavigateTo(parent.FullName);
        else
            NavigateTo(CurrentPath); // корень диска — обновить
    }

    // Обновить содержимое
    public void Refresh() => NavigateTo(CurrentPath);

    // Список выделенных (или текущий если ничего не выделено)
    public List<FsEntry> GetSelected()
    {
        var checked_ = Entries.Where(e => e.IsSelected && e.Name != "..").ToList();
        if (checked_.Count > 0) return checked_;
        if (SelectedEntry != null && SelectedEntry.Name != "..")
            return new List<FsEntry> { SelectedEntry };
        return new List<FsEntry>();
    }
}
