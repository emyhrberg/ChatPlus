using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ChatPlus.Common.Debug;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI.Chat;

namespace ChatPlus.Core.Features.Uploads;

[Autoload(Side = ModSide.Client)]
internal class UploadManager : ModSystem
{
    public static List<Upload> Uploads { get; private set; } = [];

    public override void PostSetupContent()
    {
        InitializeUploadedTextures();
    }

    public override void Unload()
    {
        if (Uploads != null)
        {
            foreach (var u in Uploads)
                DisposeLater(u.Texture);
        }
        Uploads = null;
        UploadTagHandler.Clear();
    }
    private static void UnbindAllSamplers()
    {
        var gd = Main.instance.GraphicsDevice;
        for (int i = 0; i < 8; i++)
            gd.Textures[i] = null;
    }
    private static readonly Queue<Texture2D> pendingDispose = new();

    public override void PreUpdatePlayers()
    {
        if (pendingDispose.Count == 0)
        {
            return;
        }

        UnbindAllSamplers();

        while (pendingDispose.Count > 0)
        {
            var t = pendingDispose.Dequeue();
            try
            {
                if (t != null && !t.IsDisposed)
                {
                    t.Dispose();
                }
            }
            catch
            {
            }
        }
    }

    private static void DisposeLater(Texture2D t)
    {
        if (t != null && !t.IsDisposed)
            pendingDispose.Enqueue(t);
    }
    public static bool TryDelete(Upload upload)
    {
        try
        {
            if (upload == null)
            {
                return false;
            }

            // Remove from in-memory lists first so UI stops referencing it
            int removed = Uploads.RemoveAll(u =>
                (!string.IsNullOrEmpty(upload.Tag) && string.Equals(u.Tag, upload.Tag, StringComparison.OrdinalIgnoreCase)) ||
                (!string.IsNullOrEmpty(upload.FullFilePath) && string.Equals(u.FullFilePath, upload.FullFilePath, StringComparison.OrdinalIgnoreCase)));

            // Best-effort: remove tag mapping so stale chat tags won’t resolve to a disposed texture
            try
            {
                UploadTagHandler.Remove(upload.Tag);
            }
            catch
            {
                // no-op if your handler doesn’t expose Remove; safe to ignore
            }

            // Queue GPU object for safe disposal next tick
            DisposeLater(upload.Texture);

            // Try to delete the file on disk after we’ve detached from UI/state
            if (!string.IsNullOrEmpty(upload.FullFilePath) && File.Exists(upload.FullFilePath))
            {
                File.Delete(upload.FullFilePath);
            }

            return removed > 0;
        }
        catch (Exception ex)
        {
            Log.Error("TryDelete failed: " + ex);
            Main.NewText("Delete failed.", Color.Red);
            return false;
        }
    }

    public static void AddNewUpload(Upload upload)
    {
        int idx = Uploads.FindIndex(u =>
            !string.IsNullOrEmpty(u.Tag) &&
            string.Equals(u.Tag, upload.Tag, StringComparison.OrdinalIgnoreCase));

        if (idx >= 0)
            Uploads[idx] = upload;
        else
            Uploads.Add(upload);
    }

    public static void InitializeUploadedTextures()
    {
        string folder = Path.Combine(Main.SavePath, "ChatPlus", "Uploads");
        Directory.CreateDirectory(folder);

        var files = Directory.EnumerateFiles(folder, "*.*", SearchOption.TopDirectoryOnly)
                             .Where(f =>
                             {
                                 string e = Path.GetExtension(f);
                                 return e.Equals(".png", StringComparison.OrdinalIgnoreCase)
                                     || e.Equals(".jpg", StringComparison.OrdinalIgnoreCase)
                                     || e.Equals(".jpeg", StringComparison.OrdinalIgnoreCase);
                             })
                             .OrderBy(f => f, StringComparer.OrdinalIgnoreCase)
                             .ToList();

        Main.QueueMainThreadAction(() =>
        {
            try
            {
                var fresh = new List<Upload>(files.Count);
                UploadTagHandler.Clear();

                foreach (var file in files)
                {
                    try
                    {
                        string key = Path.GetFileNameWithoutExtension(file);
                        using var fs = File.OpenRead(file);
                        var tex = Texture2D.FromStream(Main.instance.GraphicsDevice, fs);

                        if (UploadTagHandler.Register(key, tex))
                        {
                            fresh.Add(new Upload(
                                Tag: UploadTagHandler.GenerateTag(key),
                                FileName: Path.GetFileName(file),
                                FullFilePath: file,
                                Texture: tex
                            ));
                        }
                    }
                    catch { /* ignore bad file */ }
                }

                // Swap atomically first so UI stops referencing old textures
                var old = Uploads;
                Uploads = fresh;

                // Unbind any samplers that may still reference old textures
                UnbindAllSamplers();

                // Dispose old textures on next Update tick (not right now)
                foreach (var u in old)
                    DisposeLater(u.Texture);

                Log.Info($"Uploads refreshed: {Uploads.Count} files");
            }
            catch (Exception ex)
            {
                Log.Error("InitializeUploadedTextures failed: " + ex);
            }
        });
    }
}
