using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;

namespace WpfCommander.Models
{
    public class FileSystemItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private string _name = string.Empty;
        public string Name
        {
            get => _name;
            set
            {
                if (_name == value) return;
                _name = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DisplayName));
                OnPropertyChanged(nameof(Type));
            }
        }

        private string _fullPath = string.Empty;
        public string FullPath
        {
            get => _fullPath;
            set
            {
                if (_fullPath == value) return;
                _fullPath = value;
                OnPropertyChanged();
            }
        }

        private bool _isDirectory;
        public bool IsDirectory
        {
            get => _isDirectory;
            set
            {
                if (_isDirectory == value) return;
                _isDirectory = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Type));
                OnPropertyChanged(nameof(Icon));
                OnPropertyChanged(nameof(SizeDisplay));
            }
        }

        private bool _isParentEntry;
        public bool IsParentEntry
        {
            get => _isParentEntry;
            set
            {
                if (_isParentEntry == value) return;
                _isParentEntry = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DisplayName));
                OnPropertyChanged(nameof(Type));
                OnPropertyChanged(nameof(Icon));
                OnPropertyChanged(nameof(SizeDisplay));
                OnPropertyChanged(nameof(ModifiedDisplay));
            }
        }

        private long _size;
        public long Size
        {
            get => _size;
            set
            {
                if (_size == value) return;
                _size = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SizeDisplay));
            }
        }

        private DateTime _modified;
        public DateTime Modified
        {
            get => _modified;
            set
            {
                if (_modified == value) return;
                _modified = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ModifiedDisplay));
            }
        }

        public string DisplayName => IsParentEntry ? ".." : Name;

        public string Type
        {
            get
            {
                if (IsParentEntry) return "<Вверх>";
                if (IsDirectory) return "<Папка>";
                var ext = Path.GetExtension(Name);
                return string.IsNullOrEmpty(ext) ? "Файл" : ext.TrimStart('.').ToUpperInvariant();
            }
        }

        public string SizeDisplay
        {
            get
            {
                if (IsParentEntry || IsDirectory) return string.Empty;
                return FormatSize(Size);
            }
        }

        public string ModifiedDisplay =>
            IsParentEntry || Modified == default
                ? string.Empty
                : Modified.ToString("yyyy-MM-dd HH:mm");

        public string Icon
        {
            get
            {
                if (IsParentEntry) return "↩";
                if (IsDirectory) return "📁";
                return "📄";
            }
        }

        private static string FormatSize(long bytes)
        {
            string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
            double size = bytes;
            int i = 0;
            while (size >= 1024 && i < suffixes.Length - 1)
            {
                size /= 1024;
                i++;
            }
            return i == 0 ? $"{bytes} B" : $"{size:0.##} {suffixes[i]}";
        }
    }
}
