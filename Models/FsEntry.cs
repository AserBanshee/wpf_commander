using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FileVault.Models;

// Модель одного элемента файловой системы
public class FsEntry : INotifyPropertyChanged
{
    private bool _isSelected;

    public string Name { get; set; } = "";
    public string FullPath { get; set; } = "";
    public string EntryType { get; set; } = "";   // "Папка" или расширение
    public string DisplaySize { get; set; } = ""; // отформатированный размер
    public long RawSize { get; set; }
    public DateTime Modified { get; set; }
    public bool IsDirectory { get; set; }

    public bool IsSelected
    {
        get => _isSelected;
        set { _isSelected = value; OnPropertyChanged(); }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? n = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
}
