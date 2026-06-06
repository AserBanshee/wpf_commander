using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using WpfCommander.Commands;
using WpfCommander.Models;
using WpfCommander.Services;
using WpfCommander.Views;

namespace WpfCommander.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly FileManagerService _service = new();

        public MainViewModel()
        {
            LeftPanel = new PanelViewModel(_service);
            RightPanel = new PanelViewModel(_service);

            LeftPanel.IsActive = true;
            ActivePanel = LeftPanel;
            PassivePanel = RightPanel;

            CopyCommand      = new RelayCommand(CopySelected,    HasSelectionAndPassive);
            MoveCommand      = new RelayCommand(MoveSelected,    HasSelectionAndPassive);
            DeleteCommand    = new RelayCommand(DeleteSelected,  HasSelection);
            NewFolderCommand = new RelayCommand(CreateNewFolder, HasActivePath);
        }

        public PanelViewModel LeftPanel { get; }
        public PanelViewModel RightPanel { get; }

        private PanelViewModel? _activePanel;
        public PanelViewModel? ActivePanel
        {
            get => _activePanel;
            private set => SetField(ref _activePanel, value);
        }

        private PanelViewModel? _passivePanel;
        public PanelViewModel? PassivePanel
        {
            get => _passivePanel;
            private set => SetField(ref _passivePanel, value);
        }

        public void SetActive(PanelViewModel panel)
        {
            if (panel == ActivePanel) return;
            LeftPanel.IsActive = panel == LeftPanel;
            RightPanel.IsActive = panel == RightPanel;
            ActivePanel = panel;
            PassivePanel = panel == LeftPanel ? RightPanel : LeftPanel;
            CommandManager.InvalidateRequerySuggested();
        }

        public ICommand CopyCommand      { get; }
        public ICommand MoveCommand      { get; }
        public ICommand DeleteCommand    { get; }
        public ICommand NewFolderCommand { get; }

        private bool HasSelectionAndPassive() => HasSelection() && PassivePanel != null
            && !string.IsNullOrEmpty(PassivePanel.CurrentPath);

        private bool HasSelection() => GetSelected().Any();

        private bool HasActivePath() =>
            ActivePanel != null && !string.IsNullOrEmpty(ActivePanel.CurrentPath);

        private IEnumerable<FileSystemItem> GetSelected()
        {
            if (ActivePanel == null) return Array.Empty<FileSystemItem>();

            var source = ActivePanel.SelectedItems?.ToList() ?? new List<FileSystemItem>();
            if (source.Count == 0 && ActivePanel.SelectedItem != null)
                source.Add(ActivePanel.SelectedItem);
            return source.Where(s => !s.IsParentEntry);
        }

        private void CopySelected()
        {
            var items = GetSelected().ToList();
            if (items.Count == 0 || PassivePanel == null) return;

            int ok = 0;
            foreach (var item in items)
            {
                if (TryRun(() => _service.Copy(item, PassivePanel.CurrentPath), item.Name))
                    ok++;
            }
            PassivePanel.Refresh();
            if (ActivePanel != null)
                ActivePanel.StatusMessage = $"Скопировано: {ok} из {items.Count}";
        }

        private void MoveSelected()
        {
            var items = GetSelected().ToList();
            if (items.Count == 0 || PassivePanel == null || ActivePanel == null) return;

            int ok = 0;
            foreach (var item in items)
            {
                if (TryRun(() => _service.Move(item, PassivePanel.CurrentPath), item.Name))
                    ok++;
            }
            ActivePanel.Refresh();
            PassivePanel.Refresh();
            ActivePanel.StatusMessage = $"Перемещено: {ok} из {items.Count}";
        }

        private void DeleteSelected()
        {
            var items = GetSelected().ToList();
            if (items.Count == 0 || ActivePanel == null) return;

            var msg = items.Count == 1
                ? $"Удалить '{items[0].Name}'?"
                : $"Удалить {items.Count} элементов?";

            var answer = MessageBox.Show(msg, "Подтверждение удаления",
                MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (answer != MessageBoxResult.Yes) return;

            int ok = 0;
            foreach (var item in items)
            {
                if (TryRun(() => _service.Delete(item), item.Name))
                    ok++;
            }
            ActivePanel.Refresh();
            ActivePanel.StatusMessage = $"Удалено: {ok} из {items.Count}";
        }

        private void CreateNewFolder()
        {
            if (ActivePanel == null || string.IsNullOrEmpty(ActivePanel.CurrentPath)) return;

            var dlg = new InputDialog("Новая папка", "Введите имя папки:", "Новая папка")
            {
                Owner = Application.Current.MainWindow
            };
            if (dlg.ShowDialog() != true) return;

            if (TryRun(() => _service.CreateFolder(ActivePanel.CurrentPath, dlg.ResponseText),
                       dlg.ResponseText))
            {
                ActivePanel.Refresh();
                ActivePanel.StatusMessage = $"Создана папка: {dlg.ResponseText}";
            }
        }

        private static bool TryRun(Action action, string subject)
        {
            try
            {
                action();
                return true;
            }
            catch (UnauthorizedAccessException)
            {
                ShowError("Отказано в доступе", subject);
            }
            catch (IOException ex)
            {
                ShowError(ex.Message, subject);
            }
            catch (Exception ex)
            {
                ShowError(ex.Message, subject);
            }
            return false;
        }

        private static void ShowError(string message, string subject)
        {
            MessageBox.Show($"{subject}\n\n{message}", "Ошибка",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
