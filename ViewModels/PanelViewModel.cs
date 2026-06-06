using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using WpfCommander.Commands;
using WpfCommander.Models;
using WpfCommander.Services;

namespace WpfCommander.ViewModels
{
    public class PanelViewModel : ViewModelBase
    {
        private readonly FileManagerService _service;

        public PanelViewModel(FileManagerService service)
        {
            _service = service;

            Drives = new ObservableCollection<string>(
                DriveInfo.GetDrives()
                    .Where(d => d.IsReady)
                    .Select(d => d.Name));

            OpenItemCommand = new RelayCommand<FileSystemItem>(OpenItem);
            GoUpCommand = new RelayCommand(GoUp, CanGoUp);

            if (Drives.Count > 0)
                SelectedDrive = Drives.First();
        }

        public ObservableCollection<string> Drives { get; }

        public ObservableCollection<FileSystemItem> Items { get; } = new();

        private string? _selectedDrive;
        public string? SelectedDrive
        {
            get => _selectedDrive;
            set
            {
                if (!SetField(ref _selectedDrive, value)) return;
                if (value != null && !string.Equals(value, Path.GetPathRoot(CurrentPath),
                                                    StringComparison.OrdinalIgnoreCase))
                {
                    Navigate(value);
                }
            }
        }

        private string _currentPath = string.Empty;
        public string CurrentPath
        {
            get => _currentPath;
            private set => SetField(ref _currentPath, value);
        }

        private FileSystemItem? _selectedItem;
        public FileSystemItem? SelectedItem
        {
            get => _selectedItem;
            set => SetField(ref _selectedItem, value);
        }

        public IList<FileSystemItem>? SelectedItems { get; set; }

        private bool _isActive;
        public bool IsActive
        {
            get => _isActive;
            set => SetField(ref _isActive, value);
        }

        private string? _statusMessage;
        public string? StatusMessage
        {
            get => _statusMessage;
            set => SetField(ref _statusMessage, value);
        }

        public ICommand OpenItemCommand { get; }
        public ICommand GoUpCommand { get; }

        public void Navigate(string path)
        {
            List<FileSystemItem> items;
            try
            {
                items = _service.ListDirectory(path).ToList();
            }
            catch (UnauthorizedAccessException)
            {
                StatusMessage = "Отказано в доступе";
                return;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка: {ex.Message}";
                return;
            }

            Items.Clear();
            foreach (var i in items) Items.Add(i);
            CurrentPath = path;
            StatusMessage = $"Элементов: {items.Count(x => !x.IsParentEntry)}";

            var root = Path.GetPathRoot(path);
            if (!string.IsNullOrEmpty(root) && Drives.Contains(root)
                && !string.Equals(root, _selectedDrive, StringComparison.OrdinalIgnoreCase))
            {
                _selectedDrive = root;
                OnPropertyChanged(nameof(SelectedDrive));
            }
        }

        public void Refresh()
        {
            if (!string.IsNullOrEmpty(CurrentPath))
                Navigate(CurrentPath);
        }

        private void OpenItem(FileSystemItem? item)
        {
            if (item == null) return;
            if (item.IsParentEntry || item.IsDirectory)
                Navigate(item.FullPath);
        }

        private void GoUp()
        {
            if (string.IsNullOrEmpty(CurrentPath)) return;
            var parent = Directory.GetParent(CurrentPath);
            if (parent != null) Navigate(parent.FullName);
        }

        private bool CanGoUp() =>
            !string.IsNullOrEmpty(CurrentPath) && Directory.GetParent(CurrentPath) != null;
    }
}
