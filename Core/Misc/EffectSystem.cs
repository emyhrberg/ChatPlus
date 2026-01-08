using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using ReLogic.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using static ReLogic.Graphics.DynamicSpriteFont;

namespace ChatPlus.Core.Misc;

/// <summary>
/// Initializes effects
/// </summary>
public class EffectSystem : ModSystem
{
    // Effects
    public static readonly Lazy<Asset<Effect>> boldEffect = new(
        () => ModContent.Request<Effect>("ChatPlus/Assets/Effects/BoldEffect", AssetRequestMode.ImmediateLoad)
    );
}
