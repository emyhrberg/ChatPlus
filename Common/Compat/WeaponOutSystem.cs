using System;
using System.Linq;
using System.Reflection;
using ChatPlus.Common.Debug;
using ChatPlus.Core.Features.Stats.PlayerStats;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.RuntimeDetour;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace ChatPlus.Common.Compat;

internal class WeaponOutSystem : ModSystem
{
    private Hook weaponLayerHook;

    private delegate void DrawDelegate(object self, ref PlayerDrawSet drawInfo);

    public override void Load()
    {
        if (ModLoader.TryGetMod("WeaponOutLite", out Mod mod))
        {
            HookWeaponLayer(mod);
        }
    }

    public override void Unload()
    {
        weaponLayerHook?.Dispose();
        weaponLayerHook = null;
    }

    private void HookWeaponLayer(Mod mod)
    {
        var type = mod.Code.GetType("WeaponOutLite.Common.WeaponOutItemHeldLayer");
        if (type == null)
        {
            Log.Error("WeaponOutItemHeldLayer not found.");
            return;
        }

        var method = type.GetMethod("Draw", BindingFlags.NonPublic | BindingFlags.Instance);
        if (method == null)
        {
            Log.Error("WeaponOutItemHeldLayer.Draw not found.");
            return;
        }

        weaponLayerHook = new Hook(method,(DrawDelegate orig, object self, ref PlayerDrawSet drawInfo) =>
        {
            orig(self, ref drawInfo);

            if (ModifyPlayerDrawInfo.ForceFullBrightOnce)
            {
                ModifyWeaponDrawInfo(ref drawInfo);
            }
        });
    }

    private static readonly string[] BackEffectHints = ["Wings", "Back", "RocketBoots", "Jetpack", "Rocket", "MiscEffects", "Starboard", "CelestialStarboard"];

    private void ModifyWeaponDrawInfo(ref PlayerDrawSet drawInfo)
    {
        if (!ModifyPlayerDrawInfo.ForceFullBrightOnce) return;

        Item held = drawInfo.drawPlayer.HeldItem;
        if (held != null && !held.IsAir && drawInfo.DrawDataCache != null)
        {
            for (int i = drawInfo.DrawDataCache.Count - 1; i >= 0; i--)
            {
                var dd = drawInfo.DrawDataCache[i];
                if (IsBackEffectTexture(dd.texture))
                {
                    drawInfo.DrawDataCache.RemoveAt(i);
                }
            }

            for (int i = drawInfo.DrawDataCache.Count - 1; i >= 0; i--)
            {
                var dd = drawInfo.DrawDataCache[i];
                if (IsGlowMaskTexture(dd.texture))
                {
                    dd.position = new Vector2(0, 0);

                    Vector2 pos;
                    if (PlayerInfoState.Active)
                    {
                        float x = ((Main.screenWidth - Math.Min((int)(Main.screenWidth * 0.8f), 1000)) / 2 + 42) + (Main.screenWidth < 940 ? 326 : 436) * 0.5f;
                        float y = 321f;
                        pos = new Vector2(x, y);
                    }
                    else
                    {
                        pos = Main.MouseScreen + new Vector2(130, 126);
                        const int H = 356;
                        const int W = 216;
                        Vector2 offset = new Vector2(130, 126);
                        pos.X = Math.Clamp(pos.X, offset.X - 20, Main.screenWidth - W + offset.X - 20);
                        pos.Y = Math.Clamp(pos.Y, offset.Y - 20, Main.screenHeight - H + offset.Y - 20);
                    }

                    dd.position = pos;
                    drawInfo.DrawDataCache[i] = dd;
                }
            }

            for (int i = 0; i < drawInfo.DrawDataCache.Count; i++)
            {
                DrawData weapon = drawInfo.DrawDataCache[i];

                if (weapon.texture == TextureAssets.Item[held.type].Value)
                {
                    weapon.scale *= 1.0f;
                    weapon.color = Color.White;

                    Vector2 pos;
                    if (PlayerInfoState.Active)
                    {
                        float x = ((Main.screenWidth - Math.Min((int)(Main.screenWidth * 0.8f), 1000)) / 2 + 42) + (Main.screenWidth < 940 ? 326 : 436) * 0.5f;
                        float y = 321f;
                        pos = new Vector2(x, y);
                    }
                    else
                    {
                        pos = Main.MouseScreen + new Vector2(130, 126);
                        const int H = 356;
                        const int W = 216;
                        Vector2 offset = new Vector2(130, 126);
                        pos.X = Math.Clamp(pos.X, offset.X - 20, Main.screenWidth - W + offset.X - 20);
                        pos.Y = Math.Clamp(pos.Y, offset.Y - 20, Main.screenHeight - H + offset.Y - 20);
                    }

                    weapon.position = pos;
                    drawInfo.DrawDataCache[i] = weapon;
                }
            }
        }
    }

    private static bool IsBackEffectTexture(Texture2D tex)
    {
        if (tex == null) return false;
        var name = tex.Name;
        if (string.IsNullOrEmpty(name)) return false;

        for (int i = 0; i < BackEffectHints.Length; i++)
        {
            if (name.IndexOf(BackEffectHints[i], StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsGlowMaskTexture(Texture2D tex)
    {
        if (tex == null) return false;
        var arr = TextureAssets.GlowMask;
        for (int g = 0; g < arr.Length; g++)
        {
            var asset = arr[g];
            if (asset != null && asset.IsLoaded && asset.Value == tex)
                return true;
        }
        return false;
    }
}
