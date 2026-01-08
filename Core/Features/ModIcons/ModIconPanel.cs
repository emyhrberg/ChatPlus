using System;
using System.Collections.Generic;
using System.Linq;
using ChatPlus.Core.Chat;
using ChatPlus.Core.Features.Stats.ModStats;

using ChatPlus.Core.UI;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;

namespace ChatPlus.Core.Features.ModIcons;

public class ModIconPanel : BasePanel<ModIcon>
{
    private static IReadOnlyList<LocalMod> cachedMods;
    private static double lastCacheUpdate = -1;

    private static IReadOnlyList<LocalMod> GetCachedMods()
    {
        // Refresh only once per session or if you want, after some time
        if (cachedMods == null || lastCacheUpdate < 0)
        {
            cachedMods = ModOrganizer.FindAllMods();
            lastCacheUpdate = Main.gameTimeCache.TotalGameTime.TotalSeconds;
        }
        return cachedMods;
    }
    protected override IEnumerable<ModIcon> GetSource() => ModIconManager.ModIcons;
    protected override BaseElement<ModIcon> BuildElement(ModIcon data) => new ModIconElement(data);
    protected override string GetTag(ModIcon data) => data.Tag;

    public override void Update(GameTime gt)
    {
        base.Update(gt);
    }

    public override void InsertSelectedTag()
    {
        if (items.Count == 0)
        {
            return;
        }

        if (currentIndex < 0)
        {
            return;
        }

        string insert = GetTag(items[currentIndex].Data);
        if (string.IsNullOrEmpty(insert))
        {
            return;
        }

        string text = Main.chatText ?? string.Empty;
        int caret = Math.Clamp(HandleChatSystem.GetCaretPos(), 0, text.Length);

        // 1) Open bracketed [m... fragment: replace it
        int mStart = text.LastIndexOf("[m", StringComparison.OrdinalIgnoreCase);
        if (mStart >= 0 && caret >= mStart)
        {
            int closing = text.IndexOf(']', mStart + 2);
            bool isOpen = closing == -1 || closing > caret;
            if (isOpen)
            {
                string before = text.Substring(0, mStart);
                string after = text.Substring(caret);

                bool needSpace = NeedsLeadingSpace(before);
                string space = needSpace ? " " : string.Empty;

                Main.chatText = before + space + insert + after;
                HandleChatSystem.SetCaretPos(before.Length + space.Length + insert.Length);
                return;
            }
        }

        // 2) Fallback: append with leading space if needed
        {
            string before = text;
            bool needSpace = NeedsLeadingSpace(before);
            string space = needSpace ? " " : string.Empty;

            Main.chatText = before + space + insert;
            HandleChatSystem.SetCaretPos(Main.chatText.Length);
        }

        static bool NeedsLeadingSpace(string before)
        {
            if (string.IsNullOrEmpty(before))
            {
                return false;
            }

            char prev = before[before.Length - 1];
            if (char.IsWhiteSpace(prev))
            {
                return false;
            }

            if (prev == '[')
            {
                return false;
            }

            return true;
        }
    }


    public void OpenModInfoForSelectedMod()
    {
        // Need the selected ModIcon from the connected BasePanel<ModIcon>
        if (TryGetSelected(out var entry))
        {
            var mod = entry.mod;
            if (ModInfoState.Instance == null)
            {
                Main.NewText("Mod info UI not available.", Color.Orange);
                return;
            }
            string displayName = mod?.DisplayName ?? mod?.Name ?? "Unknown Mod";
            string internalName = mod?.Name ?? displayName;

            string version = mod?.Version?.ToString() ?? "Unknown";
            string description = GetDescriptionForMod(mod);

            var state = ModInfoState.Instance;
            state.SetModInfo(description, displayName, internalName);
            state.OpenForCurrentContext();
        }
    }

    protected override string GetDescription(ModIcon data)
    {
        string result = data.mod.Name;

        if (data.mod != null && ModLoader.TryGetMod(data.mod.Name, out Mod mod))
        {
            LocalMod localMod = GetCachedMods()
                .FirstOrDefault(m => m != null && string.Equals(m.Name, mod.Name, StringComparison.OrdinalIgnoreCase));

            if (localMod?.properties != null)
            {
                result = data.mod.Name + " v" + localMod.properties.version;
            }
        }

        return result;
    }

    private string GetDescriptionForMod(Mod mod)
    {
        if (mod == null) return "No description available.";

        LocalMod localMod = GetCachedMods()
            .FirstOrDefault(m => m != null && string.Equals(m.Name, mod.Name, StringComparison.OrdinalIgnoreCase));

        string desc = localMod?.properties?.description;
        return string.IsNullOrWhiteSpace(desc) ? "" : desc;
    }
}
