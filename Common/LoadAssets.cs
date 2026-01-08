using System.Reflection;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.ModLoader;

namespace ChatPlus.Common;

/// <summary>
/// Static class to hold all assets used in the mod.
/// </summary>
public static class Ass
{
    // Add assets here
    public static Asset<Texture2D> AuthorIcon;
    public static Asset<Texture2D> FileSizeIcon;
    public static Asset<Texture2D> LastUpdatedIcon;
    public static Asset<Texture2D> VersionIcon;
    public static Asset<Texture2D> ClientIcon; // side
    public static Asset<Texture2D> ServerIcon; // side
    public static Asset<Texture2D> TypingIndicator; // spritesheet w: 32, h: 26, count: 10
    public static Asset<Texture2D> TypingIndicatorDotsOnly; // spritesheet w: 32, h: 26, count: 10

    public static Asset<Texture2D> Hitbox;
    public static Asset<Texture2D> StatPanel;
    public static Asset<Texture2D> SmallPanelHighlight;
    public static Asset<Texture2D> TerrariaIcon;
    public static Asset<Texture2D> tModLoaderIcon;
    public static Asset<Texture2D> FilterList;
    public static Asset<Texture2D> ButtonColor;
    public static Asset<Texture2D> ButtonUpload;

    // This bool automatically initializes all specified assets
    public static bool Initialized { get; set; }

    static Ass()
    {
        foreach (FieldInfo field in typeof(Ass).GetFields())
        {
            if (field.FieldType == typeof(Asset<Texture2D>))
            {
                var asset = ModContent.Request<Texture2D>($"ChatPlus/Assets/{field.Name}", AssetRequestMode.AsyncLoad);
                field.SetValue(null, asset);
            }
        }
    }
}

/// <summary>
/// System that automatically initializes assets
/// </summary>
public class LoadAssets : ModSystem
{
    public override void Load()
    {
        _ = Ass.Initialized;
    }
}