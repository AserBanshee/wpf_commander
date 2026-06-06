using System.IO;
using System.Windows;
using System.Windows.Input;
using FileVault.Commands;
using FileVault.Services;

namespace FileVault.ViewModels;

// Координирует две панели и файловые операции
public class MainViewModel : ViewModelBase
{
    private readonly DiskService _disk = new();

    public ExplorerPaneVM LeftPane { get; }
    public ExplorerPaneVM RightPane { get; }

    // Активная и пассивная панели
    private ExplorerPaneVM Active => LeftPane.IsActive ? LeftPane : RightPane;
    private ExplorerPaneVM Passive => LeftPane.IsActive ? RightPane : LeftPane;

    public ICommand CopyCommand { get; }
    public ICommand MoveCommand { get; }
    public ICommand NewFolderCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand ActivateLeftCommand { get; }
    public ICommand ActivateRightCommand { get; }

    public MainViewModel()
    {
        LeftPane = new ExplorerPaneVM(_disk) { IsActive = true };
        RightPane = new ExplorerPaneVM(_disk) { IsActive = false };

        // Правая панель открывает второй диск если есть
        if (RightPane.Drives.Count > 1)
            RightPane.NavigateTo(RightPane.Drives[1]);

        CopyCommand      = new RelayCommand(_ => ExecuteCopy());
        MoveCommand      = new RelayCommand(_ => ExecuteMove());
        NewFolderCommand = new RelayCommand(_ => ExecuteNewFolder());
        DeleteCommand    = new RelayCommand(_ => ExecuteDelete());
        ActivateLeftCommand  = new RelayCommand(_ => SetActive(LeftPane));
        ActivateRightCommand = new RelayCommand(_ => SetActive(RightPane));
    }

    public void SetActive(ExplorerPaneVM pane)
    {
        LeftPane.IsActive  = pane == LeftPane;
        RightPane.IsActive = pane == RightPane;
    }

    // F5 — копирование
    private void ExecuteCopy()
    {
        var items = Active.GetSelected();
        if (items.Count == 0) { Warn("Выберите файл или папку."); return; }

        var dest = Passive.CurrentPath;
        var errors = new List<string>();

        foreach (var item in items)
        {
            try { _disk.Copy(item.FullPath, dest); }
            catch (IOException ex) { errors.Add($"{item.Name}: {ex.Message}"); }
            catch (UnauthorizedAccessException) { errors.Add($"{item.Name}: отказано в доступе"); }
            catch (Exception ex) { errors.Add($"{item.Name}: {ex.Message}"); }
        }

        Active.Refresh();
        Passive.Refresh();
        if (errors.Count > 0)
            MessageBox.Show(string.Join("\n", errors), "Ошибки при копировании",
                MessageBoxButton.OK, MessageBoxImage.Warning);
    }

    // F6 — перемещение
    private void ExecuteMove()
    {
        var items = Active.GetSelected();
        if (items.Count == 0) { Warn("Выберите файл или папку."); return; }

        var dest = Passive.CurrentPath;
        var errors = new List<string>();

        foreach (var item in items)
        {
            try { _disk.Move(item.FullPath, dest); }
            catch (IOException ex) { errors.Add($"{item.Name}: {ex.Message}"); }
            catch (UnauthorizedAccessException) { errors.Add($"{item.Name}: отказано в доступе"); }
            catch (Exception ex) { errors.Add($"{item.Name}: {ex.Message}"); }
        }

        Active.Refresh();
        Passive.Refresh();
        if (errors.Count > 0)
            MessageBox.Show(string.Join("\n", errors), "Ошибки при перемещении",
                MessageBoxButton.OK, MessageBoxImage.Warning);
    }

    // F7 — новая папка
    private void ExecuteNewFolder()
    {
        var dlg = new Views.NamePrompt("Новая папка", "Введите имя папки:");
        if (dlg.ShowDialog() != true || string.IsNullOrWhiteSpace(dlg.EnteredName))
            return;

        try
        {
            _disk.CreateFolder(Active.CurrentPath, dlg.EnteredName.Trim());
            Active.Refresh();
        }
        catch (IOException ex)
        {
            MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    // F8 — удаление
    private void ExecuteDelete()
    {
        var items = Active.GetSelected();
        if (items.Count == 0) { Warn("Выберите файл или папку."); return; }

        var names = string.Join("\n", items.Select(i => i.Name));
        var confirm = MessageBox.Show(
            $"Удалить следующие объекты?\n\n{names}",
            "Подтверждение удаления",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (confirm != MessageBoxResult.Yes) return;

        var errors = new List<string>();
        foreach (var item in items)
        {
            try { _disk.Delete(item.FullPath); }
            catch (IOException ex) { errors.Add($"{item.Name}: {ex.Message}"); }
            catch (UnauthorizedAccessException) { errors.Add($"{item.Name}: отказано в доступе"); }
            catch (Exception ex) { errors.Add($"{item.Name}: {ex.Message}"); }
        }

        Active.Refresh();
        if (errors.Count > 0)
            MessageBox.Show(string.Join("\n", errors), "Ошибки при удалении",
                MessageBoxButton.OK, MessageBoxImage.Warning);
    }

    private static void Warn(string msg)
        => MessageBox.Show(msg, "FileVault", MessageBoxButton.OK, MessageBoxImage.Information);
}
