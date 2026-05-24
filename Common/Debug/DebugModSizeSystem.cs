using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;

namespace ChatPlus.Common.Debug;

public class DebugModSizeSystem : ModSystem
{
    private sealed class FileEntry
    {
        public string Path = "";
        public string Folder = "";
        public string FileName = "";
        public string Extension = "";
        public long SizeBytes;
    }

    public override void PostSetupContent()
    {
        if (Main.dedServ)
            return;

        try
        {
            LogPackagedAssetSizes();
        }
        catch (Exception ex)
        {
            Log.Debug($"Failed: {ex}");
        }
    }

    private void LogPackagedAssetSizes()
    {
        List<string> files = Mod.GetFileNames();
        List<FileEntry> entries = [];

        Dictionary<string, long> exactFolderSizes = new(StringComparer.OrdinalIgnoreCase);
        Dictionary<string, long> cumulativeFolderSizes = new(StringComparer.OrdinalIgnoreCase);
        Dictionary<string, long> extensionSizes = new(StringComparer.OrdinalIgnoreCase);

        foreach (string rawName in files)
        {
            string path = NormalizePath(rawName);
            byte[] bytes = Mod.GetFileBytes(path);

            if (bytes == null)
                continue;

            long size = bytes.LongLength;
            string folder = GetFolder(path);
            string fileName = GetFileName(path);
            string extension = GetExtension(path);

            entries.Add(new FileEntry
            {
                Path = path,
                Folder = string.IsNullOrEmpty(folder) ? "<root>" : folder,
                FileName = fileName,
                Extension = extension,
                SizeBytes = size
            });

            AddExactFolderSize(exactFolderSizes, folder, size);
            AddCumulativeFolderSizes(cumulativeFolderSizes, folder, size);

            if (!extensionSizes.TryAdd(extension, size))
                extensionSizes[extension] += size;
        }

        long totalLogicalBytes = entries.Sum(x => x.SizeBytes);

        Mod.Logger.Debug("==================================================");
        Mod.Logger.Debug($"[{Mod.Name}] Packaged asset size report");
        Mod.Logger.Debug("==================================================");
        Mod.Logger.Debug($"Files: {entries.Count}");
        Mod.Logger.Debug($"Logical total: {FormatBytes(totalLogicalBytes)}");
        Mod.Logger.Debug("Note: logical total is useful for ranking, not exact final .tmod contribution.");
        Mod.Logger.Debug("==================================================");

        Mod.Logger.Debug($"[{Mod.Name}] Top 150 biggest packaged files");
        Mod.Logger.Debug("==================================================");

        int rank = 1;
        foreach (FileEntry entry in entries
                     .OrderByDescending(x => x.SizeBytes)
                     .ThenBy(x => x.Path)
                     .Take(150))
        {
            Mod.Logger.Debug($"{rank,3}. {FormatBytes(entry.SizeBytes),10}  {entry.Path}");
            rank++;
        }

        Mod.Logger.Debug("==================================================");
        Mod.Logger.Debug($"[{Mod.Name}] Totals by extension");
        Mod.Logger.Debug("==================================================");

        foreach (var kv in extensionSizes
                     .OrderByDescending(x => x.Value)
                     .ThenBy(x => x.Key))
        {
            string extension = string.IsNullOrEmpty(kv.Key) ? "<no extension>" : kv.Key;
            Mod.Logger.Debug($"{FormatBytes(kv.Value),10}  {extension}");
        }

        Mod.Logger.Debug("==================================================");
        Mod.Logger.Debug($"[{Mod.Name}] Exact folder sizes");
        Mod.Logger.Debug("==================================================");

        foreach (var kv in exactFolderSizes
                     .OrderByDescending(x => x.Value)
                     .ThenBy(x => x.Key))
        {
            string folder = string.IsNullOrEmpty(kv.Key) ? "<root>" : kv.Key;
            Mod.Logger.Debug($"{FormatBytes(kv.Value),10}  {folder}");
        }

        Mod.Logger.Debug("==================================================");
        Mod.Logger.Debug($"[{Mod.Name}] Cumulative folder sizes");
        Mod.Logger.Debug("==================================================");

        foreach (var kv in cumulativeFolderSizes
                     .OrderByDescending(x => x.Value)
                     .ThenBy(x => x.Key))
        {
            string folder = string.IsNullOrEmpty(kv.Key) ? "<root>" : kv.Key;
            Mod.Logger.Debug($"{FormatBytes(kv.Value),10}  {folder}");
        }

        Mod.Logger.Debug("==================================================");
        Mod.Logger.Debug($"[{Mod.Name}] Top 100 biggest textures (.rawimg)");
        Mod.Logger.Debug("==================================================");

        rank = 1;
        foreach (FileEntry entry in entries
                     .Where(x => x.Extension.Equals(".rawimg", StringComparison.OrdinalIgnoreCase))
                     .OrderByDescending(x => x.SizeBytes)
                     .ThenBy(x => x.Path)
                     .Take(100))
        {
            Mod.Logger.Debug($"{rank,3}. {FormatBytes(entry.SizeBytes),10}  {entry.Path}");
            rank++;
        }

        Mod.Logger.Debug("==================================================");
    }

    private static void AddExactFolderSize(Dictionary<string, long> folderSizes, string folder, long size)
    {
        if (!folderSizes.TryAdd(folder, size))
            folderSizes[folder] += size;
    }

    private static void AddCumulativeFolderSizes(Dictionary<string, long> folderSizes, string folder, long size)
    {
        if (!folderSizes.TryAdd("", size))
            folderSizes[""] += size;

        if (string.IsNullOrEmpty(folder))
            return;

        string current = folder;
        while (true)
        {
            if (!folderSizes.TryAdd(current, size))
                folderSizes[current] += size;

            int slash = current.LastIndexOf('/');
            if (slash < 0)
                break;

            current = current[..slash];
        }
    }

    private static string NormalizePath(string path)
    {
        return path.Replace('\\', '/');
    }

    private static string GetFolder(string path)
    {
        int slash = path.LastIndexOf('/');
        return slash >= 0 ? path[..slash] : "";
    }

    private static string GetFileName(string path)
    {
        int slash = path.LastIndexOf('/');
        return slash >= 0 ? path[(slash + 1)..] : path;
    }

    private static string GetExtension(string path)
    {
        int dot = path.LastIndexOf('.');
        return dot >= 0 ? path[dot..] : "";
    }

    private static string FormatBytes(long bytes)
    {
        string[] units = ["B", "KB", "MB", "GB"];
        double size = bytes;
        int unit = 0;

        while (size >= 1024d && unit < units.Length - 1)
        {
            size /= 1024d;
            unit++;
        }

        return $"{size:0.##} {units[unit]}";
    }
}