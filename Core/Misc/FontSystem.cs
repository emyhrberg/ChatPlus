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
/// Automatically initializes fonts in Assets/Fonts folder.
/// </summary>
public class FontSystem : ModSystem
{
    // Fonts
    public static DynamicSpriteFont Bold;
    public static DynamicSpriteFont Italics;
    public static DynamicSpriteFont BoldShader => bold.Value;

    // Effects
    private static readonly Lazy<Asset<Effect>> boldEffect = new(
        () => ModContent.Request<Effect>("ChatPlus/Assets/Effects/BoldEffect", AssetRequestMode.ImmediateLoad)
    );

    private static readonly Lazy<DynamicSpriteFont> bold =
        new(() => MakeBoldFont(FontAssets.MouseText.Value, characterSpace: 1.05f, lineSpace: 1.05f, boldRadius: 0.5f, kernalSize: 1.1f));


    public override void PostSetupContent()
    {
        // Fonts
        Bold = ModContent.Request<DynamicSpriteFont>("ChatPlus/Assets/Fonts/Bold", AssetRequestMode.ImmediateLoad).Value;
        Italics = ModContent.Request<DynamicSpriteFont>("ChatPlus/Assets/Fonts/Italics", AssetRequestMode.ImmediateLoad).Value;
    }

    public static DynamicSpriteFont MakeBoldFont(DynamicSpriteFont font,float characterSpace = 1f,float lineSpace = 1f,float boldRadius = 1f,float kernalSize = 1f)
    {
        Effect effect = boldEffect.Value.Value;

        float newSpacing = font.CharacterSpacing * characterSpace;
        int newLineSpace = (int)(font.LineSpacing * lineSpace);

        var newLetterMap = new Dictionary<char, SpriteCharacterData>(font.SpriteCharacters.Count);
        SpriteCharacterData newDefaultLetter = default;
        char newDefaultChar = (char)0;

        Dictionary<Texture2D, Texture2D> oldToNew = [];

        foreach (var pair in font.SpriteCharacters)
        {
            Texture2D oldImage = pair.Value.Texture;
            if (!oldToNew.TryGetValue(oldImage, out Texture2D newImage))
            {
                var theNewImage = new RenderTarget2D(Main.instance.GraphicsDevice, oldImage.Width, oldImage.Height);
                var oldTargets = Main.instance.GraphicsDevice.GetRenderTargets();
                Main.instance.GraphicsDevice.SetRenderTarget(theNewImage);
                Main.instance.GraphicsDevice.Clear(Color.Transparent);

                using (var drawer = new SpriteBatch(Main.instance.GraphicsDevice))
                {
                    drawer.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null);
                    effect.Parameters["imageWidth"].SetValue(oldImage.Width);
                    effect.Parameters["imageHeight"].SetValue(oldImage.Height);
                    effect.Parameters["boldRadius"].SetValue(boldRadius);
                    effect.CurrentTechnique.Passes[0].Apply();
                    drawer.Draw(oldImage, Vector2.Zero, Color.White);
                    drawer.End();
                }

                Main.instance.GraphicsDevice.SetRenderTargets(oldTargets);
                newImage = theNewImage;
                oldToNew.Add(oldImage, newImage);
            }

            var newGlyph = pair.Value.Glyph;
            newGlyph.X -= 1;
            newGlyph.Y -= 1;
            newGlyph.Width += 2;
            newGlyph.Height += 2;

            var newLetter = new SpriteCharacterData(newImage, newGlyph, pair.Value.Padding, pair.Value.Kerning * kernalSize);
            newLetterMap.Add(pair.Key, newLetter);

            if (newDefaultChar == 0 && oldImage == font.DefaultCharacterData.Texture)
            {
                newDefaultChar = pair.Key;
                newDefaultLetter = newLetter;
            }
        }

        var newFont = new DynamicSpriteFont(newSpacing, newLineSpace, newDefaultChar)
        {
            _spriteCharacters = newLetterMap,
            _defaultCharacterData = newDefaultLetter
        };

        return newFont;
    }
}
