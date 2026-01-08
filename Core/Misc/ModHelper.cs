using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;

namespace ChatPlus.Core.Misc;
public static class ModHelper
{
    public static LocalMod GetLocalMod(Mod mod)
    {
        string modName = mod.Name;

        LocalMod[] mods = ModOrganizer.FindAllMods();
        LocalMod localMod = Array.Find(mods, m => m.Name.Equals(modName, StringComparison.OrdinalIgnoreCase));

        if (localMod == null) return null;

        return localMod;
    }

    public static string GetDisplayName(string name)
    {
        return ModLoader.TryGetMod(name, out var mod) ? (mod.DisplayName ?? name) : name;
    }
}
public static class ModMetaCache
{
    // name -> (size, lastWriteUtc, path)
    private static readonly Dictionary<string, (long size, DateTime lastWriteUtc, string path)> _cache = new();

    public static (long size, DateTime lastWriteUtc, string path) Get(Mod mod)
    {
        if (mod == null) return default;

        if (_cache.TryGetValue(mod.Name, out var v))
            return v;

        LocalMod local = ModOrganizer.FindAllMods().FirstOrDefault(m =>
            m.Name.Equals(mod.Name, StringComparison.OrdinalIgnoreCase));

        if (local == null)
        {
            _cache[mod.Name] = default;
            return default;
        }

        string path = local.modFile?.path ?? string.Empty;
        long size = 0;
        DateTime last = DateTime.MinValue;

        if (!string.IsNullOrEmpty(path) && File.Exists(path))
        {
            var fi = new FileInfo(path);
            size = fi.Length;
            last = fi.LastWriteTimeUtc;
        }

        v = (size, last, path);
        _cache[mod.Name] = v;
        return v;
    }
}


