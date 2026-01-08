using ChatPlus.Core.UI;
using Microsoft.Xna.Framework.Graphics;
using Terraria.UI;

namespace ChatPlus.Core.Features.Glyphs;

public class GlyphState : BaseState<Glyph>
{
    public GlyphState() : base(new GlyphPanel(), new DescriptionPanel<Glyph>())
    {
    }
}
