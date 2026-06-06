using System.IO;
using FileVault.Models;

namespace FileVault.Services;

// Вся файловая логика вынесена сюда
public class DiskService
{
    // Список логических дисков
    public List<string> GetDrives()
        => DriveInfo.GetDrives()
                    .Where(d => d.IsReady)
                    .Select(d => d.Name)
                    .ToList();

    // Содержимое директории
    public List<FsEntry> GetEntries(string path)
    {
        var result = new List<FsEntry>();

        // Элемент подъёма на уровень выше
        var parent = Directory.GetParent(path);
        if (parent != null)
            result.Add(new FsEntry
            {
                Name = "..",
                FullPath = parent.FullName,
                EntryType = "",
                DisplaySize = "",
                IsDirectory = true,
                Modified = DateTime.MinValue
            });

        try
        {
            // Папки
            foreach (var dir in Directory.GetDirectories(path))
            {
                try
                {
                    var info = new DirectoryInfo(dir);
                    result.Add(new FsEntry
                    {
                        Name = info.Name,
                        FullPath = info.FullName,
                        EntryType = "Папка",
                        DisplaySize = "<Папка>",
                        IsDirectory = true,
                        Modified = info.LastWriteTime
                    });
                }
                catch { /* нет доступа к папке — пропускаем */ }
            }

            // Файлы
            foreach (var file in Directory.GetFiles(path))
            {
                try
                {
                    var info = new FileInfo(file);
                    result.Add(new FsEntry
                    {
                        Name = info.Name,
                        FullPath = info.FullName,
                        EntryType = string.IsNullOrEmpty(info.Extension)
                                        ? "Файл"
                                        : info.Extension.TrimStart('.').ToUpper(),
                        DisplaySize = FormatSize(info.Length),
                        RawSize = info.Length,
                        IsDirectory = false,
                        Modified = info.LastWriteTime
                    });
                }
                catch { /* нет доступа к файлу — пропускаем */ }
            }
        }
        catch (UnauthorizedAccessException)
        {
            throw new UnauthorizedAccessException("Отказано в доступе");
        }

        return result;
    }

    // Копирование файла или папки
    public void Copy(string source, string destDir)
    {
        var name = Path.GetFileName(source);
        var dest = Path.Combine(destDir, name);

        if (Directory.Exists(source))
            CopyDir(source, dest);
        else
            File.Copy(source, dest, overwrite: false);
    }

    // Перемещение файла или папки
    public void Move(string source, string destDir)
    {
        var name = Path.GetFileName(source);
        var dest = Path.Combine(destDir, name);

        if (Directory.Exists(source))
            Directory.Move(source, dest);
        else
            File.Move(source, dest, overwrite: false);
    }

    // Создание папки
    public void CreateFolder(string parentPath, string folderName)
    {
        var full = Path.Combine(parentPath, folderName);
        if (Directory.Exists(full))
            throw new IOException($"Папка «{folderName}» уже существует");
        Directory.CreateDirectory(full);
    }

    // Удаление файла или папки
    public void Delete(string path)
    {
        if (Directory.Exists(path))
            Directory.Delete(path, recursive: true);
        else
            File.Delete(path);
    }

    // Рекурсивное копирование папки
    private static void CopyDir(string src, string dst)
    {
        Directory.CreateDirectory(dst);
        foreach (var f in Directory.GetFiles(src))
            File.Copy(f, Path.Combine(dst, Path.GetFileName(f)), overwrite: false);
        foreach (var d in Directory.GetDirectories(src))
            CopyDir(d, Path.Combine(dst, Path.GetFileName(d)));
    }

    // Форматирование размера
    public static string FormatSize(long bytes)
    {
        if (bytes < 1024) return $"{bytes} Б";
        if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F1} КБ";
        if (bytes < 1024 * 1024 * 1024) return $"{bytes / 1024.0 / 1024:F1} МБ";
        return $"{bytes / 1024.0 / 1024 / 1024:F2} ГБ";
    }
}
