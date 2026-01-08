using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader.UI;

namespace ChatPlus.Common.Configs.ConfigElements.PlayerColor;

internal class CustomColoredImageButton : UIColoredImageButton
{
    public string tooltip;
    public CustomColoredImageButton(Asset<Texture2D> texture, string tooltip) : base(texture, true)
    {
        _color = Color.White;
        _texture = texture;

        _backPanelTexture = Main.Assets.Request<Texture2D>("Images/UI/CharCreation/SmallPanel");
        _backPanelHighlightTexture = Ass.SmallPanelHighlight;
        _backPanelBorderTexture = Main.Assets.Request<Texture2D>("Images/UI/CharCreation/SmallPanelBorder");

        Width.Set(_backPanelTexture.Width(), 0f);
        Height.Set(_backPanelTexture.Height(), 0f);

        this.tooltip = tooltip;
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        base.Draw(spriteBatch);

        if (IsMouseHovering)
        {
            UICommon.TooltipMouseText(tooltip);
        }
    }
}

