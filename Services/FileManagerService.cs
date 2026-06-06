using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WpfCommander.Models;

namespace WpfCommander.Services
{
    public class FileManagerService
    {
        public IEnumerable<FileSystemItem> ListDirectory(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("Путь не задан.", nameof(path));
            if (!Directory.Exists(path))
                throw new DirectoryNotFoundException($"Папка не найдена: {path}");

            var result = new List<FileSystemItem>();

            var parent = Directory.GetParent(path);
            if (parent != null)
            {
                result.Add(new FileSystemItem
                {
                    Name = "..",
                    FullPath = parent.FullName,
                    IsDirectory = true,
                    IsParentEntry = true
                });
            }

            var directories = Directory.EnumerateDirectories(path)
                .Select(p =>
                {
                    try { return new DirectoryInfo(p); }
                    catch { return null; }
                })
                .Where(d => d != null)
                .OrderBy(d => d!.Name, StringComparer.CurrentCultureIgnoreCase);

            foreach (var info in directories)
            {
                DateTime modified;
                try { modified = info!.LastWriteTime; }
                catch { modified = default; }

                result.Add(new FileSystemItem
                {
                    Name = info!.Name,
                    FullPath = info.FullName,
                    IsDirectory = true,
                    Modified = modified
                });
            }

            var files = Directory.EnumerateFiles(path)
                .Select(p =>
                {
                    try { return new FileInfo(p); }
                    catch { return null; }
                })
                .Where(f => f != null)
                .OrderBy(f => f!.Name, StringComparer.CurrentCultureIgnoreCase);

            foreach (var info in files)
            {
                long size;
                DateTime modified;
                try { size = info!.Length; modified = info.LastWriteTime; }
                catch { size = 0; modified = default; }

                result.Add(new FileSystemItem
                {
                    Name = info!.Name,
                    FullPath = info.FullName,
                    IsDirectory = false,
                    Size = size,
                    Modified = modified
                });
            }

            return result;
        }

        public void Copy(FileSystemItem source, string destinationDir)
        {
            ValidateSource(source);
            ValidateDestinationDir(destinationDir);

            var target = Path.Combine(destinationDir, source.Name);
            EnsureNotSame(source.FullPath, target);

            if (source.IsDirectory)
            {
                if (Directory.Exists(target))
                    throw new IOException($"Папка '{source.Name}' уже существует в целевой папке.");
                if (IsSubPath(source.FullPath, destinationDir))
                    throw new IOException("Нельзя скопировать папку внутрь самой себя.");
                CopyDirectory(source.FullPath, target);
            }
            else
            {
                if (File.Exists(target))
                    throw new IOException($"Файл '{source.Name}' уже существует в целевой папке.");
                File.Copy(source.FullPath, target, overwrite: false);
            }
        }

        public void Move(FileSystemItem source, string destinationDir)
        {
            ValidateSource(source);
            ValidateDestinationDir(destinationDir);

            var target = Path.Combine(destinationDir, source.Name);
            EnsureNotSame(source.FullPath, target);

            if (source.IsDirectory)
            {
                if (Directory.Exists(target))
                    throw new IOException($"Папка '{source.Name}' уже существует в целевой папке.");
                if (IsSubPath(source.FullPath, destinationDir))
                    throw new IOException("Нельзя переместить папку внутрь самой себя.");
                Directory.Move(source.FullPath, target);
            }
            else
            {
                if (File.Exists(target))
                    throw new IOException($"Файл '{source.Name}' уже существует в целевой папке.");
                File.Move(source.FullPath, target);
            }
        }

        public void Delete(FileSystemItem item)
        {
            ValidateSource(item);

            if (item.IsDirectory)
                Directory.Delete(item.FullPath, recursive: true);
            else
                File.Delete(item.FullPath);
        }

        public void CreateFolder(string parentDir, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Имя папки не может быть пустым.");
            if (name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
                throw new ArgumentException("Имя содержит недопустимые символы.");
            if (!Directory.Exists(parentDir))
                throw new DirectoryNotFoundException($"Целевая папка не существует: {parentDir}");

            var target = Path.Combine(parentDir, name);
            if (Directory.Exists(target) || File.Exists(target))
                throw new IOException($"Элемент '{name}' уже существует.");

            Directory.CreateDirectory(target);
        }

        private static void CopyDirectory(string source, string destination)
        {
            Directory.CreateDirectory(destination);

            foreach (var file in Directory.EnumerateFiles(source))
            {
                var target = Path.Combine(destination, Path.GetFileName(file));
                File.Copy(file, target, overwrite: false);
            }

            foreach (var dir in Directory.EnumerateDirectories(source))
            {
                var target = Path.Combine(destination, Path.GetFileName(dir));
                CopyDirectory(dir, target);
            }
        }

        private static void ValidateSource(FileSystemItem source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (source.IsParentEntry)
                throw new InvalidOperationException("Нельзя выполнить операцию над элементом '..'.");
            if (source.IsDirectory && !Directory.Exists(source.FullPath))
                throw new DirectoryNotFoundException($"Папка не найдена: {source.FullPath}");
            if (!source.IsDirectory && !File.Exists(source.FullPath))
                throw new FileNotFoundException($"Файл не найден: {source.FullPath}");
        }

        private static void ValidateDestinationDir(string destinationDir)
        {
            if (string.IsNullOrWhiteSpace(destinationDir))
                throw new ArgumentException("Не задан целевой каталог.");
            if (!Directory.Exists(destinationDir))
                throw new DirectoryNotFoundException($"Целевая папка не существует: {destinationDir}");
        }

        private static void EnsureNotSame(string source, string target)
        {
            if (string.Equals(Path.GetFullPath(source), Path.GetFullPath(target),
                              StringComparison.OrdinalIgnoreCase))
                throw new IOException("Источник и приёмник совпадают.");
        }

        private static bool IsSubPath(string ancestor, string descendant)
        {
            var a = Path.GetFullPath(ancestor).TrimEnd(Path.DirectorySeparatorChar);
            var d = Path.GetFullPath(descendant).TrimEnd(Path.DirectorySeparatorChar);
            return d.StartsWith(a + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)
                   || string.Equals(a, d, StringComparison.OrdinalIgnoreCase);
        }
    }
}
